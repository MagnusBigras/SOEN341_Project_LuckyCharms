//local storage helpers
const STORAGE_KEYS = {
  orgs: "admin_portal_orgs",
  roles: "admin_portal_roles",
  restrictions: "admin_portal_restrictions",
};

const saveLS = (k, v) => localStorage.setItem(k, JSON.stringify(v));
const loadLS = (k, fallback) => {
  try { return JSON.parse(localStorage.getItem(k)) ?? fallback; }
  catch { return fallback; }
};

//seed data

let organizations = loadLS(STORAGE_KEYS.orgs, [
  { id: crypto.randomUUID(), name: "Tech Corp", users: 58, active: true },
  { id: crypto.randomUUID(), name: "Health Plus", users: 42, active: true },
  { id: crypto.randomUUID(), name: "EduFuture", users: 25, active: false },
]);

// roles: { [email]: "User" | "Organizator" | "Admin" }
let roles = loadLS(STORAGE_KEYS.roles, {
  "admin@example.com": "Admin",
  "org@example.com": "Organizator",
});

// restrictions: { [email]: { type: "none"|"banned"|"suspended", until?: ISO } }
let restrictions = loadLS(STORAGE_KEYS.restrictions, {
  "banned@example.com": { type: "banned" },
});


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
    nameTd.innerHTML = `
      <span class="name-text">${org.name}</span>
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
  Object.entries(restrictions).forEach(([email, r]) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `<td>${email}</td><td>${restrictionLabel(r)}</td>`;
    restrictionsTableBody.appendChild(tr);
  });
}

/* -------------------------------------------------
   Orgs: add / edit / delete / toggle
------------------------------------------------- */
addOrgBtn.addEventListener("click", () => {
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
  organizations.unshift({ id: crypto.randomUUID(), name, users: 0, active: true });
  saveLS(STORAGE_KEYS.orgs, organizations);
  orgNameInput.value = "";
  renderOrgs();
});

// event delegation for actions
orgTableBody.addEventListener("click", (e) => {
  const tr = e.target.closest("tr");
  if(!tr) return;
  const id = tr.dataset.id;
  const org = organizations.find(o => o.id === id);
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
    const input = tr.querySelector(".edit-input");
    const newName = (input.value || "").trim();
    orgErrors.classList.add("hidden");
    if(!newName){
      orgErrors.textContent = "Organization name cannot be empty.";
      orgErrors.classList.remove("hidden");
      return;
    }
    if(organizations.some(o => o.id !== id && o.name.toLowerCase() === newName.toLowerCase())){
      orgErrors.textContent = "Duplicate organization name.";
      orgErrors.classList.remove("hidden");
      return;
    }
    org.name = newName;
    saveLS(STORAGE_KEYS.orgs, organizations);
    renderOrgs();
  }

  // cancel edit
  if(e.target.closest(".cancel-edit")){
    renderOrgs();
  }

  // delete
  if(e.target.closest(".delete-org")){
    organizations = organizations.filter(o => o.id !== id);
    saveLS(STORAGE_KEYS.orgs, organizations);
    renderOrgs();
  }

  // toggle active
  if(e.target.classList.contains("switch")){
    org.active = !org.active;
    saveLS(STORAGE_KEYS.orgs, organizations);
    renderOrgs();
  }
});

//ROLES ASSIGN
assignRoleBtn.addEventListener("click", () => {
  roleErrors.classList.add("hidden");
  const email = (roleEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    roleErrors.textContent = "Please enter a valid email address.";
    roleErrors.classList.remove("hidden");
    return;
  }
  const role = roleSelect.value;
  roles[email] = role;
  saveLS(STORAGE_KEYS.roles, roles);
  roleEmail.value = "";
  renderRoles();
});

// Restrictions: ban / unban / suspend 

banBtn.addEventListener("click", () => {
  restrErrors.classList.add("hidden");
  const email = (restrEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    restrErrors.textContent = "Please enter a valid email address.";
    restrErrors.classList.remove("hidden");
    return;
  }
  restrictions[email] = { type:"banned" };
  saveLS(STORAGE_KEYS.restrictions, restrictions);
  renderRestrictions();
});

unbanBtn.addEventListener("click", () => {
  restrErrors.classList.add("hidden");
  const email = (restrEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    restrErrors.textContent = "Please enter a valid email address.";
    restrErrors.classList.remove("hidden");
    return;
  }
  restrictions[email] = { type:"none" };
  saveLS(STORAGE_KEYS.restrictions, restrictions);
  renderRestrictions();
});

suspendBtn.addEventListener("click", () => {
  restrErrors.classList.add("hidden");
  const email = (restrEmail.value || "").trim().toLowerCase();
  if(!emailOk(email)){
    restrErrors.textContent = "Please enter a valid email address.";
    restrErrors.classList.remove("hidden");
    return;
  }
  const val = suspendDuration.value;
  let until = null;
  if(val === "1d") until = addDays(new Date(), 1);
  else if(val === "7d") until = addDays(new Date(), 7);
  else if(val === "30d") until = addDays(new Date(), 30);
  else if(val === "perm") until = null;

  restrictions[email] = { type: "suspended", ...(until ? { until: until.toISOString() } : {}) };
  saveLS(STORAGE_KEYS.restrictions, restrictions);
  renderRestrictions();
});

function addDays(d, days){
  const x = new Date(d);
  x.setDate(x.getDate() + days);
  return x;
}


renderOrgs();
renderRoles();
renderRestrictions();
