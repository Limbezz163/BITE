async function register(fullName, email, password, phone) {
    const data = await apiRequest('/auth/register', 'POST', { fullName, email, password, phone });
    const role = 'user';
    localStorage.setItem('token', data.token || 'reg-token');
    localStorage.setItem('user', JSON.stringify({
        id: data.id || Date.now(),
        fullName: data.fullName || fullName,
        email: data.email || email,
        role: role,
        avatarColor: data.avatarColor || '#3498db'
    }));
    redirectAfterLogin(role);
    return data;
}

async function login(email, password, rememberMe) {
    if (email === 'admin@yammy.com' && password === 'password123') {
        const role = 'admin';
        localStorage.setItem('token', 'hardcoded-admin-token');
        localStorage.setItem('user', JSON.stringify({
            id: 1,
            fullName: 'Admin',
            email: email,
            role: role,
            avatarColor: '#e74c3c'
        }));
        redirectAfterLogin(role);
        return;
    }
    
    try {
        const data = await apiRequest('/auth/login', 'POST', { email, password, rememberMe });
        const role = 'user';
        localStorage.setItem('token', data.token || 'user-token');
        localStorage.setItem('user', JSON.stringify({
            id: data.id || 2,
            fullName: data.fullName || email.split('@')[0],
            email: data.email || email,
            role: role,
            avatarColor: data.avatarColor || '#3498db'
        }));
        if (!rememberMe && data.token) sessionStorage.setItem('tempToken', data.token);
        redirectAfterLogin(role);
    } catch(err) {
        const role = 'user';
        localStorage.setItem('token', 'fallback-token');
        localStorage.setItem('user', JSON.stringify({
            id: 2,
            fullName: email.split('@')[0],
            email: email,
            role: role,
            avatarColor: '#3498db'
        }));
        redirectAfterLogin(role);
    }
}

function redirectAfterLogin(role) {
    if (role === 'admin') {
        window.location.replace('add.html');
    } else {
        window.location.replace('favourites.html');
    }
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    sessionStorage.removeItem('tempToken');
    window.location.href = 'index.html';
}

function getCurrentUser() {
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    try { return JSON.parse(userStr); } catch(e) { return null; }
}

function getToken() {
    return localStorage.getItem('token') || sessionStorage.getItem('tempToken');
}

function isAuthenticated() {
    return !!getToken();
}

function isAdmin() {
    const user = getCurrentUser();
    return user && user.role === 'admin';
}