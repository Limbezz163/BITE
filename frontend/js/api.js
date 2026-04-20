const API_BASE = 'http://localhost:5000/api';

async function apiRequest(endpoint, method = 'GET', body = null, requireAuth = false) {
    const headers = { 'Content-Type': 'application/json' };
    if (requireAuth) {
        const token = localStorage.getItem('token');
        if (!token) throw new Error('Необходима авторизация');
        headers['Authorization'] = `Bearer ${token}`;
    }
    const config = { method, headers };
    if (body) config.body = JSON.stringify(body);
    const response = await fetch(`${API_BASE}${endpoint}`, config);
    if (!response.ok) {
        let errorMessage = `Ошибка ${response.status}`;
        try {
            const errorData = await response.json();
            errorMessage = errorData.message || errorMessage;
        } catch(e) {}
        throw new Error(errorMessage);
    }
    if (response.status === 204) return null;
    return await response.json();
}