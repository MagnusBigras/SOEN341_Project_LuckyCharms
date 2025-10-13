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

// Submission feedback (mock)
document.getElementById("loginForm").addEventListener("submit", (e) => {
  e.preventDefault();
  alert("Welcome back!");
});

document.getElementById("registerForm").addEventListener("submit", (e) => {
  e.preventDefault();
  alert("Successful Account Registration");
});

document.getElementById("forgotForm").addEventListener("submit", (e) => {
  e.preventDefault();
  alert("Password reset instructions sent! Check you Inbox");
});
