const loginContainer = document.querySelector(".login-container");
const registerContainer = document.querySelector(".register-container");
const forgotContainer = document.querySelector(".forgot-container");

const showRegister = document.getElementById("showRegister");
const showLogin = document.getElementById("showLogin");
const showForgot = document.getElementById("showForgot");
const backToLogin = document.getElementById("backToLogin");

// Switch to Register
showRegister.addEventListener("click", (e) => {
  e.preventDefault();
  loginContainer.classList.add("hidden");
  registerContainer.classList.remove("hidden");
  forgotContainer.classList.add("hidden");
});

// Switch to Login from Register
showLogin.addEventListener("click", (e) => {
  e.preventDefault();
  registerContainer.classList.add("hidden");
  loginContainer.classList.remove("hidden");
  forgotContainer.classList.add("hidden");
});

// Switch to Forgot Password
showForgot.addEventListener("click", (e) => {
  e.preventDefault();
  loginContainer.classList.add("hidden");
  registerContainer.classList.add("hidden");
  forgotContainer.classList.remove("hidden");
});

// Back to Login from Forgot Password
backToLogin.addEventListener("click", (e) => {
  e.preventDefault();
  forgotContainer.classList.add("hidden");
  loginContainer.classList.remove("hidden");
  registerContainer.classList.add("hidden");
});

document.getElementById('registerForm').addEventListener('submit', async function (e) {
    e.preventDefault();
    const form = e.target;

    const payload = {
        FirstName: form.querySelector('#FirstName')?.value || '',
        LastName: form.querySelector('#LastName')?.value || '',
        PhoneNumber: form.querySelector('#PhoneNumber')?.value || '',
        DateOfBirth: form.querySelector('#DateOfBirth')?.value || '',
        Email: form.querySelector('#Email')?.value || '',
        UserName: form.querySelector('#UserName')?.value || '',
        Password: form.querySelector('#Password')?.value || '',
        PasswordSalt: "Test",
        AccountCreationDate: new Date().toISOString(),
        AccountType: 0
    };
    console.log("Sending payload:", payload);
    try {
        const response = await fetch('/api/accounts/create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            const result = await response.json();
            console.log("Account created:", result);
            alert("Account created successfully!");
            // Optionally redirect or reset form
        } else {
            const error = await response.text();
            console.error("Failed to create account:", error);
            alert("Error creating account: " + error);
        }
    } catch (error) {
        console.error("Network or server error:", error);
        alert("Something went wrong. Please try again.");
    }
});
