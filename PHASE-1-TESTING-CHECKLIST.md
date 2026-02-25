# ✅ Phase 1 Testing Checklist

**Complete this checklist to finish Phase 1!**

> **Status Update (Feb 24, 2026):** Core end-to-end authentication flow validated successfully (register → login → dashboard → logout → protected route + token checks).

---

## 🎯 **OBJECTIVE**
Test the complete authentication flow from React frontend → .NET API → AWS RDS

---

## 📋 **PRE-TESTING SETUP**

### Step 1: Verify Backend is Running
- [ ] Open Visual Studio
- [ ] Identity API is running (F5)
- [ ] Console shows: "Now listening on: https://localhost:7010"
- [ ] Swagger accessible at: https://localhost:7010/swagger

### Step 2: Start Frontend
- [ ] Open VS Code
- [ ] Terminal in `ecommerce-frontend` folder
- [ ] Run: `npm start`
- [ ] Browser opens to: http://localhost:3000
- [ ] See login page

---

## 🧪 **TEST SCENARIO 1: User Registration**

### Test 1.1: Valid Registration
- [ ] Click "Register here" link
- [ ] Fill form:
  - First Name: `Test`
  - Last Name: `User`
  - Email: `test.user@example.com`
  - Password: `SecurePass@123`
  - Confirm Password: `SecurePass@123`
- [ ] Click "Register" button
- [ ] See loading spinner
- [ ] See success toast notification
- [ ] Redirected to `/dashboard`
- [ ] Dashboard shows user information

**Expected Result:**
```
✅ Registration successful!
✅ JWT token received
✅ User data stored in AWS RDS
✅ Redirected to dashboard
✅ User info displayed correctly
```

### Test 1.2: Duplicate Email Validation
- [ ] Logout from dashboard
- [ ] Go to Register page
- [ ] Try to register with same email: `test.user@example.com`
- [ ] Click "Register"
- [ ] See error message: "User with email ... already exists"
- [ ] Error toast notification appears

**Expected Result:**
```
❌ Registration rejected
✅ Clear error message
✅ User stays on registration page
```

### Test 1.3: Password Mismatch Validation
- [ ] Fill registration form
- [ ] Password: `Test@123`
- [ ] Confirm Password: `Test@456` (different)
- [ ] Click "Register"
- [ ] See error: "Passwords do not match"

**Expected Result:**
```
❌ Form validation caught mismatch
✅ Error message shown
✅ No API call made
```

### Test 1.4: Weak Password Validation
- [ ] Fill registration form
- [ ] Password: `123` (too short)
- [ ] Click "Register"
- [ ] See error: "Password must be at least 6 characters long"

**Expected Result:**
```
❌ Form validation caught weak password
✅ Error message shown
```

---

## 🧪 **TEST SCENARIO 2: User Login**

### Test 2.1: Valid Login
- [ ] Navigate to Login page
- [ ] Fill form:
  - Email: `test.user@example.com`
  - Password: `SecurePass@123`
- [ ] Click "Login" button
- [ ] See loading spinner
- [ ] See success toast notification
- [ ] Redirected to `/dashboard`
- [ ] Dashboard shows correct user data

**Expected Result:**
```
✅ Login successful!
✅ JWT token received
✅ Redirected to dashboard
✅ User info displayed
✅ Last login time updated
```

### Test 2.2: Invalid Password
- [ ] Go to Login page
- [ ] Fill form:
  - Email: `test.user@example.com`
  - Password: `WrongPassword`
- [ ] Click "Login"
- [ ] See error: "Invalid email or password"
- [ ] Error toast notification appears
- [ ] Stay on login page

**Expected Result:**
```
❌ Login rejected
✅ Clear error message
✅ No token received
```

### Test 2.3: Non-existent User
- [ ] Try to login with email that doesn't exist
- [ ] Email: `nonexistent@example.com`
- [ ] Password: `AnyPassword`
- [ ] See error: "Invalid email or password"

**Expected Result:**
```
❌ Login rejected
✅ Error message (doesn't reveal if user exists - security!)
```

---

## 🧪 **TEST SCENARIO 3: Protected Routes**

### Test 3.1: Dashboard Access Without Login
- [ ] Open new browser tab (or incognito)
- [ ] Navigate directly to: http://localhost:3000/dashboard
- [ ] Should be redirected to `/login`
- [ ] See login page

**Expected Result:**
```
❌ Access denied
✅ Redirected to login page
✅ Dashboard not accessible
```

### Test 3.2: Dashboard After Login
- [ ] Login with valid credentials
- [ ] Go to Dashboard
- [ ] See user profile:
  - Name
  - Email
  - Role(s)
  - Created date
  - Last login

**Expected Result:**
```
✅ Dashboard accessible
✅ All user data displayed
✅ Avatar with initials shown
✅ Success message displayed
```

---

## 🧪 **TEST SCENARIO 4: Logout**

### Test 4.1: Logout Functionality
- [ ] From dashboard, click "Logout" button
- [ ] Redirected to `/login`
- [ ] Try to access `/dashboard` again
- [ ] Should be redirected to `/login`

**Expected Result:**
```
✅ Logged out successfully
✅ Tokens removed from localStorage
✅ Cannot access protected routes
✅ Back to login page
```

### Test 4.2: Login After Logout
- [ ] Login again with same credentials
- [ ] Access dashboard
- [ ] Everything works normally

**Expected Result:**
```
✅ Can login again after logout
✅ New tokens issued
✅ Dashboard accessible again
```

---

## 🧪 **TEST SCENARIO 5: Token Management**

### Test 5.1: Token Storage
- [ ] Login successfully
- [ ] Open browser DevTools (F12)
- [ ] Go to "Application" tab (Chrome) or "Storage" tab (Firefox)
- [ ] Check localStorage
- [ ] See: `accessToken`, `refreshToken`, `user`

**Expected Result:**
```
✅ Tokens stored in localStorage
✅ User object stored
✅ All data present
```

### Test 5.2: Token in API Requests
- [ ] Stay logged in
- [ ] Open DevTools → Network tab
- [ ] Reload dashboard (triggers profile API call)
- [ ] Click on the API request
- [ ] Check Headers tab
- [ ] See: `Authorization: Bearer eyJ...`

**Expected Result:**
```
✅ Token sent in Authorization header
✅ API accepts the token
✅ Profile data returned
```

---

## 🧪 **TEST SCENARIO 6: Data Verification**

### Test 6.1: Verify in AWS RDS
```powershell
sqlcmd -S ecommerce-identity-db.cd8iom6664t2.eu-north-1.rds.amazonaws.com,1433 -U admin -P "k1kJDXlHyqgUS2SEmxBX" -d ecommerce-identity-db -Q "SELECT Email, FirstName, LastName, Roles, IsActive, CreatedAt FROM Users"
```

- [ ] Run the command
- [ ] See your registered users
- [ ] Verify email matches
- [ ] Verify names match
- [ ] Verify roles = ["Customer"]
- [ ] Verify IsActive = true

**Expected Result:**
```
✅ User data in AWS RDS
✅ All fields correct
✅ Passwords hashed (not visible)
✅ Roles stored as JSON
```

---

## 🧪 **TEST SCENARIO 7: Error Handling**

### Test 7.1: Backend API Down
- [ ] Stop the backend API (Stop debugging in Visual Studio)
- [ ] Try to login from React
- [ ] Should see error: "Network Error" or "API not reachable"

**Expected Result:**
```
❌ Cannot connect to API
✅ Clear error message shown
✅ App doesn't crash
```

### Test 7.2: Invalid API URL
- [ ] In `.env`, change to wrong URL
- [ ] Restart React app
- [ ] Try to login
- [ ] See connection error

**Expected Result:**
```
❌ Cannot reach backend
✅ Error handled gracefully
```

---

## 🧪 **TEST SCENARIO 8: UI/UX Testing**

### Test 8.1: Form Validation
- [ ] Try to submit empty registration form
- [ ] See required field errors
- [ ] Try invalid email format
- [ ] See email validation error

### Test 8.2: Loading States
- [ ] Click register/login
- [ ] See loading spinner on button
- [ ] Button disabled during loading

### Test 8.3: Notifications
- [ ] Successful registration shows green toast
- [ ] Successful login shows green toast
- [ ] Errors show red toast
- [ ] Toasts auto-dismiss after 3 seconds

### Test 8.4: Navigation
- [ ] Can navigate between Login and Register
- [ ] Links work correctly
- [ ] Browser back button works
- [ ] URL changes correctly

---

## ✅ **PHASE 1 COMPLETION CRITERIA**

### **All Green? Phase 1 is DONE!** 🎉

- [ ] **Registration works** - User can create account
- [ ] **Data persists** - Stored in AWS RDS
- [ ] **Login works** - User can authenticate
- [ ] **JWT works** - Token generated and validated
- [ ] **Dashboard works** - Shows user profile
- [ ] **Logout works** - Clears tokens and redirects
- [ ] **Protection works** - Cannot access dashboard without auth
- [ ] **Validation works** - Form errors caught
- [ ] **Error handling works** - API errors displayed
- [ ] **Token refresh works** - Auto-refreshes expired tokens
- [ ] **UI is beautiful** - Material-UI looks professional
- [ ] **No console errors** - Clean browser console

---

## 🎊 **WHEN ALL TESTS PASS**

### **You Will Have:**
- ✅ Complete authentication system
- ✅ React → .NET → AWS working perfectly
- ✅ Production-ready code
- ✅ Clean architecture
- ✅ Beautiful UI
- ✅ Secure authentication
- ✅ Cloud database
- ✅ Token management

### **Next Phase:**
Move to `PHASE-2-PLAN.md` for Redis, rate limiting, and production features!

---

## 📝 **TESTING NOTES**

Use this section to note any issues found:

### Issues Found:
```
Date: 
Issue: 
Resolution: 

Date:
Issue:
Resolution:
```

---

## 🎯 **CURRENT STATUS**

**Phase 1 Progress:** 90%

**Last Tested:** [Fill in after testing]

**Test Results:** 
- Registration: [ ] Pass / [ ] Fail
- Login: [ ] Pass / [ ] Fail  
- Dashboard: [ ] Pass / [ ] Fail
- Logout: [ ] Pass / [ ] Fail
- Protection: [ ] Pass / [ ] Fail

**Blockers:** [None / List any issues]

---

> **🚀 START TESTING NOW!**  
> Run `npm start` and begin checking off items!

---

**Remember:** Check each box as you complete it. When all boxes are checked, Phase 1 is complete! 🎉
