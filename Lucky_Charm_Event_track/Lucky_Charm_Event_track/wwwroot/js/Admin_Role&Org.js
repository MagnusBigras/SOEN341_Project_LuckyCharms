// NOTE: localStorage usage removed for roles/orgs/restrictions; data comes from the server when available
// Diagnostic load marker - safe and non-intrusive. If you don't see this in the console then the script didn't run.
console.debug('Admin_Role&Org.js loaded');

//seed data

let organizations = []; // populated from the database via API

// roles: { [email]: "User" | "Organizator" | "Admin" }
// roles are fetched from the server; the client no longer stores role overrides in localStorage
let roles = {};
let accountsByEmail = {}; // map email -> account object (includes Id and AccountType)

async function loadRolesFromServer(){
  try{
  // fetch lightweight summary directly from DB via server endpoint to avoid preserved-reference wrappers
  const res = await fetch('/api/accounts/summary');
    if(!res.ok) throw new Error('Failed to fetch accounts');
    const accounts = await res.json();
    console.debug('loadRolesFromServer: fetched accounts', accounts);
    // handle cases where JSON serializer wrapped arrays (ReferenceHandler.Preserve -> {$id, $values})
    let accountsList = accounts;
    if (!Array.isArray(accountsList)) {
      accountsList = accountsList?.$values ?? accountsList?.Values ?? accountsList?.value ?? accountsList ?? null;
      // If $values is an object with numeric keys (not a true array), convert to an array
      if (accountsList && !Array.isArray(accountsList) && typeof accountsList === 'object') {
        accountsList = Object.values(accountsList);
      }
      if (!Array.isArray(accountsList)) {
        // last resort: try extracting array-like entries that look like accounts
        const maybe = Object.values(accounts || {}).filter(v => v && (v.email || v.Email));
        accountsList = maybe.length ? maybe : [];
      }
    }
    accountsByEmail = {};
    roles = {};
  console.debug('loadRolesFromServer: normalized accountsList', accountsList);
  accountsList.forEach(acc => {
        // be tolerant of JSON property casing and enum representation
        const emailRaw = acc.email || acc.Email;
        if(!emailRaw) return;
        const email = String(emailRaw).toLowerCase();

        const id = acc.id || acc.Id;
        accountsByEmail[email] = { ...(acc || {}), id };

        // AccountType may be serialized as a string ("Administrator") or number (0/1/2)
        let acctTypeRaw = acc.accountType ?? acc.AccountType;
        let acctTypeStr = '';
        if (acctTypeRaw === undefined || acctTypeRaw === null) acctTypeStr = '';
        else if (typeof acctTypeRaw === 'number') {
          // map numeric enum values -> names (follow AccountTypes enum)
          const enumMap = {
            0: 'GeneralUser',
            1: 'EventOrganizer',
            2: 'Administrator'
          };
          acctTypeStr = enumMap[acctTypeRaw] || String(acctTypeRaw);
        } else {
          acctTypeStr = String(acctTypeRaw);
        }

        let roleStr = 'User';
        const t = acctTypeStr.toLowerCase();
        if(t.includes('administrator')) roleStr = 'Admin';
        else if(t.includes('eventorganizer') || t.includes('event_organizer')) roleStr = 'Organizator';
        else roleStr = 'User';
        roles[email] = roleStr;
      });

  // No local overrides applied — server is authoritative

  }catch(err){
    console.error('Could not load accounts from server.', err);
    if(roleErrors){
      roleErrors.textContent = 'Could not load accounts from server: ' + (err && err.message ? err.message : err);
      roleErrors.classList.remove('hidden');
    }
    // fallback: no roles available
    roles = {};
  }

  renderRoles();
}

// restrictions: { [email]: { type: "none"|"banned"|"suspended", until?: ISO } }
// NOTE: client-side restrictions are kept in-memory only until a server-backed model exists
let restrictions = {};


// Orgs
const orgTableBody = document.querySelector("#orgTable tbody");
const orgNameInput = document.getElementById("orgNameInput");
const addOrgBtn = document.getElementById("addOrgBtn");
const orgErrors = document.getElementById("orgErrors");

// Roles
const roleEmail = document.getElementById("roleEmail");
const roleSelect = document.getElementById("roleSelect");
const assignRoleBtn = document.getElementById("assignRoleBtn");
const roleErrors = document.getElementById("roleErrors");
const rolesTableBody = document.querySelector("#rolesTable tbody");

// Restrictions
const restrEmail = document.getElementById("restrEmail");
const restrErrors = document.getElementById("restrErrors");
const suspendDuration = document.getElementById("suspendDuration");
const banBtn = document.getElementById("banBtn");
const unbanBtn = document.getElementById("unbanBtn");
const suspendBtn = document.getElementById("suspendBtn");
const restrictionsTableBody = document.querySelector("#restrictionsTable tbody");

//RENDER HELPERS
const emailOk = (e) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(e);

function renderOrgs(){
  orgTableBody.innerHTML = "";
  organizations.forEach(org => {
    const tr = document.createElement("tr");
    tr.dataset.id = org.id;

    // name cell (with inline edit support)
    const nameTd = document.createElement("td");
    const subtitle = org.organizerUserName && org.organizerUserName !== org.name ? `<div class="muted small">Owner: ${org.organizerUserName}</div>` : '';
    nameTd.innerHTML = `
      <div>
        <span class="name-text">${org.name}</span>
        ${subtitle}
      </div>
      <div class="edit-wrap hidden">
        <input class="edit-input" type="text" value="${org.name}">
        <button class="btn sm save-edit">Save</button>
        <button class="btn sm ghost cancel-edit">Cancel</button>
      </div>
    `;

    // users
    const usersTd = document.createElement("td");
    usersTd.textContent = org.users;

    // status
    const statusTd = document.createElement("td");
    statusTd.innerHTML = `
      <span class="status-badge ${org.active ? "status-active" : "status-inactive"}">
        ${org.active ? "Active" : "Inactive"}
      </span>
    `;

    // actions
    const actionsTd = document.createElement("td");
    actionsTd.className = "actions";
    actionsTd.innerHTML = `
      <button class="btn sm edit-org">Edit</button>
      <button class="btn sm danger delete-org">Delete</button>
      <div class="switch ${org.active ? "on" : ""}" title="Activate / Deactivate"></div>
    `;

    tr.appendChild(nameTd);
    tr.appendChild(usersTd);
    tr.appendChild(statusTd);
    tr.appendChild(actionsTd);
    orgTableBody.appendChild(tr);
  });
}

function renderRoles(){
  rolesTableBody.innerHTML = "";
  Object.entries(roles).forEach(([email, role]) => {
    const tr = document.createElement("tr");
    // Show only the role text in the roles table (no restriction/status metadata)
    tr.innerHTML = `<td>${email}</td><td>${role}</td>`;
    rolesTableBody.appendChild(tr);
  });
}

function restrictionLabel(r){
  if(!r || r.type === "none") return "None";
  if(r.type === "banned") return "Banned";
  if(r.type === "suspended"){
    if(!r.until) return "Suspended";
    const d = new Date(r.until);
    return `Suspended until ${d.toLocaleDateString()} ${d.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}`;
  }
  return "None";
}
function renderRestrictions(){
  restrictionsTableBody.innerHTML = "";
  // render computed restrictions
  Object.entries(restrictions).forEach(([email, r]) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `<td>${email}</td><td>${restrictionLabel(r)}</td>`;
    restrictionsTableBody.appendChild(tr);
  });
}

// Load restrictions (banned / suspended) from the server by reading account fields
async function loadRestrictionsFromServer(){
  try{
    // Try a few endpoints in order of preference so this works even if the server wasn't restarted
    const tryEndpoints = ['/api/accounts/restrictions','/api/accounts/all','/api/accounts/summary'];
    let data = null;
    let usedEndpoint = null;
    for(const ep of tryEndpoints){
      try{
        const r = await fetch(ep);
        if(!r.ok) { console.debug('loadRestrictionsFromServer: endpoint', ep, 'returned', r.status); continue; }
        data = await r.json();
        usedEndpoint = ep;
        break;
      }catch(err){
        console.debug('loadRestrictionsFromServer: fetch to', ep, 'failed', err && err.message ? err.message : err);
        continue;
      }
    }
    if(!data){
      throw new Error('No endpoint returned data');
    }
    console.debug('loadRestrictionsFromServer: using endpoint', usedEndpoint, 'raw data=', data);
    // normalize shapes (ReferenceHandler.Preserve)
    let list = data;
    if(!Array.isArray(list)){
      list = data?.$values ?? data?.Values ?? data ?? [];
      if(list && !Array.isArray(list) && typeof list === 'object') list = Object.values(list);
      if(!Array.isArray(list)) list = [];
    }
    restrictions = {};
  console.debug('loadRestrictionsFromServer: fetched accounts list', list);
    list.forEach(acc => {
      const emailRaw = acc.email || acc.Email || acc.UserName || acc.userName || acc.EmailAddress || acc.emailAddress;
      if(!emailRaw) return;
      const email = String(emailRaw).toLowerCase();
      const isBanned = acc.isBanned ?? acc.IsBanned ?? false;
      const suspensionRaw = acc.suspensionEndUtc ?? acc.SuspensionEndUtc ?? acc.SuspensionEndDate ?? null;
      if(isBanned){
        restrictions[email] = { type: 'banned' };
      } else if(suspensionRaw){
        // parse date and only treat as suspended if in the future
        const until = new Date(suspensionRaw);
        if(!isNaN(until) && until > new Date()){
          restrictions[email] = { type: 'suspended', until: until.toISOString() };
        } else {
          // expired or invalid -> none
          // ensure any previous entry is cleared
        }
      }
    });
    console.debug('loadRestrictionsFromServer: computed restrictions map', restrictions);
    renderRestrictions();
    // Also re-render the roles table so any newly-loaded restriction status appears next to roles
    try { renderRoles(); } catch(e) { console.debug('renderRoles failed after restrictions load', e); }
      // Update visible debug block (if present) so we can see the computed map on the page
      // no visible debug output in page (cleanup)
  }catch(err){
    console.warn('Could not load restrictions from server', err);
  }
}

/* -------------------------------------------------
   Orgs: add / edit / delete / toggle
------------------------------------------------- */
if (addOrgBtn) addOrgBtn.addEventListener("click", () => {
  const name = (orgNameInput.value || "").trim();
  orgErrors.classList.add("hidden");
  if(!name){
    orgErrors.textContent = "Organization name cannot be empty.";
    orgErrors.classList.remove("hidden");
    return;
  }
  if(organizations.some(o => o.name.toLowerCase() === name.toLowerCase())){
    orgErrors.textContent = "Duplicate organization name.";
    orgErrors.classList.remove("hidden");
    return;
  }
  // Organization creation must be done via the backend API. For now, inform the admin.
  orgErrors.textContent = "Organization changes are managed via the server. Use the backend API to create organizations.";
  orgErrors.classList.remove("hidden");
  return;
});

// event delegation for actions
if (orgTableBody) orgTableBody.addEventListener("click", (e) => {
  const tr = e.target.closest("tr");
  if(!tr) return;
  const id = tr.dataset.id;
  // dataset values are strings; compare as strings so numeric ids also match
  const org = organizations.find(o => String(o.id) === String(id));
  if(!org) return;

  // edit start
  if(e.target.closest(".edit-org")){
    const nameText = tr.querySelector(".name-text");
    const editWrap = tr.querySelector(".edit-wrap");
    nameText.classList.add("hidden");
    editWrap.classList.remove("hidden");
  }

  // save edit
  if(e.target.closest(".save-edit")){
    (async () => {
      const saveBtn = e.target.closest('.save-edit');
      const cancelBtn = tr.querySelector('.cancel-edit');
      const input = tr.querySelector(".edit-input");
      const newName = (input.value || "").trim();
      orgErrors.classList.add("hidden");
      if(!newName){
        orgErrors.textContent = "Organization name cannot be empty.";
        orgErrors.classList.remove("hidden");
        return;
      }
      if(organizations.some(o => String(o.id) !== String(id) && o.name.toLowerCase() === newName.toLowerCase())){
        orgErrors.textContent = "Duplicate organization name.";
        orgErrors.classList.remove("hidden");
        return;
      }
      try{
        if(saveBtn) saveBtn.disabled = true;
        if(cancelBtn) cancelBtn.disabled = true;
        const resp = await fetch('/api/organization/update', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ Id: Number(id), Name: newName })
        });
        if(!resp.ok){
          const txt = await resp.text().catch(() => null);
          orgErrors.textContent = 'Failed to update organization: ' + (txt || resp.status);
          orgErrors.classList.remove('hidden');
          if(saveBtn) saveBtn.disabled = false;
          if(cancelBtn) cancelBtn.disabled = false;
          return;
        }
        await loadOrgsFromServer();
      }catch(err){
        console.error('Update org failed', err);
        orgErrors.textContent = 'Update failed: ' + (err && err.message ? err.message : err);
        orgErrors.classList.remove('hidden');
        if(saveBtn) saveBtn.disabled = false;
        if(cancelBtn) cancelBtn.disabled = false;
      }
    })();
  }

  // cancel edit
  if(e.target.closest(".cancel-edit")){
    renderOrgs();
  }

  // delete
  if(e.target.closest(".delete-org")){
    (async () => {
      orgErrors.classList.add('hidden');
      const delBtn = e.target.closest('.delete-org');
      if(!delBtn) return;
      // confirm
      const confirmDel = window.confirm(`Delete organization '${org.name}'? This cannot be undone.`);
      if(!confirmDel) return;
      try{
        delBtn.disabled = true;
        const resp = await fetch('/api/organization/delete', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(Number(id))
        });
        if(!resp.ok){
          const txt = await resp.text().catch(()=>null);
          orgErrors.textContent = 'Failed to delete organization: ' + (txt || resp.status);
          orgErrors.classList.remove('hidden');
          return;
        }
        // refresh list
        await loadOrgsFromServer();
      }catch(err){
        console.error('Delete org failed', err);
        orgErrors.textContent = 'Delete failed: ' + (err && err.message ? err.message : err);
        orgErrors.classList.remove('hidden');
      }finally{
        delBtn.disabled = false;
      }
    })();
    return;
  }

  // toggle active
  if(e.target.classList.contains("switch")){
    // toggle active state via server
    (async () => {
      orgErrors.classList.add('hidden');
      const switchEl = e.target.closest('.switch');
      if(!switchEl) return;
      // prevent double clicks
      if(switchEl.dataset.busy === '1') return;
      switchEl.dataset.busy = '1';
      const newActive = !org.active;
      try{
        const resp = await fetch('/api/organization/update', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ Id: Number(id), Name: org.name || '', IsActive: newActive })
        });
        if(!resp.ok){
          const txt = await resp.text().catch(()=>null);
          orgErrors.textContent = 'Failed to update organization activation: ' + (txt || resp.status);
          orgErrors.classList.remove('hidden');
          return;
        }
        // refresh organizations from server to get authoritative state
        await loadOrgsFromServer();
      }catch(err){
        console.error('Toggle org active failed', err);
        orgErrors.textContent = 'Failed to update organization activation: ' + (err && err.message ? err.message : err);
        orgErrors.classList.remove('hidden');
      }finally{
        delete switchEl.dataset.busy;
      }
    })();
    return;
  }
});

//ROLES ASSIGN
if (assignRoleBtn) assignRoleBtn.addEventListener("click", () => {
  (async () => {
    roleErrors.classList.add("hidden");
    const email = (roleEmail.value || "").trim().toLowerCase();
    if(!emailOk(email)){
      roleErrors.textContent = "Please enter a valid email address.";
      roleErrors.classList.remove("hidden");
      return;
    }
    const role = roleSelect.value;

    // attempt to find user id from fetched accounts
    const acc = accountsByEmail[email];
    const mapToAccountType = (r) => {
      if(r === 'Admin') return 'Administrator';
      if(r === 'Organizator') return 'EventOrganizer';
      return 'GeneralUser';
    };
    if(acc && acc.id){
      try{
        const resp = await fetch('/api/accounts/assign-role', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ UserId: acc.id, Role: mapToAccountType(role) })
        });
        if(!resp.ok){
          const txt = await resp.text();
          roleErrors.textContent = `Failed to assign role on server: ${txt}`;
          roleErrors.classList.remove("hidden");
          return;
        }
            // success -> update UI from server. Remove any local override so server becomes authoritative
            // reload authoritative roles from server
            roleEmail.value = "";
            await loadRolesFromServer();
            return;
      }catch(err){
        console.warn('Assign role request failed', err);
        roleErrors.textContent = 'Assign role request failed (network).';
        roleErrors.classList.remove("hidden");
        return;
      }
    }

    // No account found in DB — inform admin (do not save local overrides)
    roleErrors.textContent = 'User not found in the database. Please ensure the email exists.';
    roleErrors.classList.remove("hidden");
    return;
  })();
});

// Restrictions: ban / unban / suspend 
// helper to call the restrict endpoint
async function callRestrictApi(userId, action, untilUtc = null) {
  const payload = { UserId: userId, Action: action };
  if (untilUtc) payload.UntilUtc = untilUtc;
  const resp = await fetch('/api/accounts/restrict', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
  if (!resp.ok) {
    const txt = await resp.text().catch(() => null);
    throw new Error(txt || `Server returned ${resp.status}`);
  }
  try { return await resp.json(); } catch { return null; }
}

if (banBtn) banBtn.addEventListener("click", async () => {
  restrErrors.classList.add("hidden");
  const email = (restrEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    restrErrors.textContent = "Please enter a valid email address.";
    restrErrors.classList.remove("hidden");
    return;
  }
  const acc = accountsByEmail[email];
  if(!acc || !acc.id){
    restrErrors.textContent = "User not found in the database.";
    restrErrors.classList.remove("hidden");
    return;
  }
  banBtn.disabled = true;
  try{
    await callRestrictApi(acc.id, 'ban', null);
  await loadRolesFromServer();
  await loadOrgsFromServer();
  await loadRestrictionsFromServer();
  restrErrors.classList.add('hidden');
  restrEmail.value = '';
  }catch(err){
    console.error('Ban failed', err);
    restrErrors.textContent = 'Ban failed: ' + (err && err.message ? err.message : err);
    restrErrors.classList.remove('hidden');
  }finally{ banBtn.disabled = false; }
});

if (unbanBtn) unbanBtn.addEventListener("click", async () => {
  restrErrors.classList.add("hidden");
  const email = (restrEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    restrErrors.textContent = "Please enter a valid email address.";
    restrErrors.classList.remove("hidden");
    return;
  }
  const acc = accountsByEmail[email];
  if(!acc || !acc.id){
    restrErrors.textContent = "User not found in the database.";
    restrErrors.classList.remove("hidden");
    return;
  }
  unbanBtn.disabled = true;
  try{
    await callRestrictApi(acc.id, 'unban', null);
  await loadRolesFromServer();
  await loadOrgsFromServer();
  await loadRestrictionsFromServer();
  restrErrors.classList.add('hidden');
  restrEmail.value = '';
  }catch(err){
    console.error('Unban failed', err);
    restrErrors.textContent = 'Unban failed: ' + (err && err.message ? err.message : err);
    restrErrors.classList.remove('hidden');
  }finally{ unbanBtn.disabled = false; }
});

if (suspendBtn) suspendBtn.addEventListener("click", async () => {
  restrErrors.classList.add("hidden");
  const email = (restrEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    restrErrors.textContent = "Please enter a valid email address.";
    restrErrors.classList.remove("hidden");
    return;
  }
  const acc = accountsByEmail[email];
  if(!acc || !acc.id){
    restrErrors.textContent = "User not found in the database.";
    restrErrors.classList.remove("hidden");
    return;
  }
  const val = suspendDuration.value;
  let until = null;
  if(val === "1d") until = addDays(new Date(), 1);
  else if(val === "7d") until = addDays(new Date(), 7);
  else if(val === "30d") until = addDays(new Date(), 30);
  else if(val === "perm") until = null;

  suspendBtn.disabled = true;
  try{
    const untilIso = until ? until.toISOString() : null;
    await callRestrictApi(acc.id, 'suspend', untilIso);
  await loadRolesFromServer();
  await loadOrgsFromServer();
  await loadRestrictionsFromServer();
  restrErrors.classList.add('hidden');
  restrEmail.value = '';
  }catch(err){
    console.error('Suspend failed', err);
    restrErrors.textContent = 'Suspend failed: ' + (err && err.message ? err.message : err);
    restrErrors.classList.remove('hidden');
  }finally{ suspendBtn.disabled = false; }
});

function addDays(d, days){
  const x = new Date(d);
  x.setDate(x.getDate() + days);
  return x;
}

// Load organizations from server
async function loadOrgsFromServer(){
  try{
    // fetch organizations (server-side Organization model)
    const res = await fetch('/api/organization/all');
    if(!res.ok) throw new Error('Failed to fetch organizations');
    let data = await res.json();
    // normalize ReferenceHandler.Preserve shapes
    let list = data;
    if(!Array.isArray(list)){
      list = data?.$values ?? data?.Values ?? data ?? [];
      if(list && !Array.isArray(list) && typeof list === 'object') list = Object.values(list);
      if(!Array.isArray(list)) list = [];
    }
    organizations = list.map(o => {
      const id = o.id ?? o.Id ?? 0;
      const active = (o.isActive ?? o.IsActive);
      const usersCount = o.currentUserCount ?? o.CurrentUserCount ?? o.currentUsers ?? o.CurrentUsers ?? 0;
      // Prefer the Organization.Name; if missing, fall back to the owning organizer's username
      const organizerUserName = o.organizer?.account?.userName ?? o.Organizer?.Account?.UserName ?? o.organizer?.userName ?? o.Organizer?.UserName ?? null;
      const name = (o.name ?? o.Name) || organizerUserName || `org-${id}`;
      const maxUsers = o.maxUsers ?? o.MaxUsers ?? 0;
      return { id, name, users: usersCount, maxUsers, active: active === undefined ? true : !!active, organizerUserName };
    });
  }catch(err){
    console.warn('Could not load organizations from server, falling back to empty list.', err);
    organizations = [];
    orgErrors.textContent = 'Could not load organizations from server.';
    orgErrors.classList.remove('hidden');
  }
  renderOrgs();
}


// bootstrap: load server-backed data then render
(async () => {
  await loadOrgsFromServer();
  await loadRolesFromServer();
  await loadRestrictionsFromServer();
})();

// debug helpers removed in cleanup
