let currentFilters = {
    search: '',
    cuisineType: '',
    restaurantType: '',
    priceRange: '',
    minPrice: null,
    maxPrice: null,
    minRating: null
};

async function loadRestaurants() {
    const params = new URLSearchParams();
    if (currentFilters.search) params.append('search', currentFilters.search);
    if (currentFilters.cuisineType) params.append('cuisineType', currentFilters.cuisineType);
    if (currentFilters.restaurantType) params.append('restaurantType', currentFilters.restaurantType);
    if (currentFilters.priceRange) params.append('priceRange', currentFilters.priceRange);
    if (currentFilters.minPrice) params.append('minPrice', currentFilters.minPrice);
    if (currentFilters.maxPrice) params.append('maxPrice', currentFilters.maxPrice);
    if (currentFilters.minRating) params.append('minRating', currentFilters.minRating);
    const queryString = params.toString();
    const url = `/restaurants${queryString ? '?' + queryString : ''}`;
    const restaurants = await apiRequest(url);
    renderRestaurants(restaurants);
}

function renderRestaurants(restaurants) {
    const container = document.getElementById('restaurants-list');
    if (!container) return;
    if (!restaurants || restaurants.length === 0) {
        container.innerHTML = '<div class="card text-center">Рестораны не найдены</div>';
        return;
    }
    container.innerHTML = restaurants.map(rest => `
        <div class="restaurant-item" data-id="${rest.id}">
            <img src="${rest.imageUrl || 'https://via.placeholder.com/300x200?text=No+Image'}" alt="${rest.name}">
            <div class="restaurant-info">
                <p><a href="restaurant.html?id=${rest.id}">${rest.name}</a></p>
                <div class="restaurant-meta">
                    <div class="rating">★ ${rest.rating}</div>
                    <div class="price">${rest.priceRange || '—'}</div>
                    <div class="cuisine">${rest.cuisineType || '—'}</div>
                </div>
                <p>${rest.shortDescription || (rest.Description ? rest.Description.substring(0, 100) + '...' : '')}</p>
                <button class="favorite-toggle-btn" data-id="${rest.id}">${rest.isFavourite ? '❤️' : '♡'}</button>
            </div>
        </div>
    `).join('');
    attachFavoriteButtons();
}

async function attachFavoriteButtons() {
    const favSet = await loadFavouritesSet();
    document.querySelectorAll('.favorite-toggle-btn').forEach(btn => {
        const restId = parseInt(btn.dataset.id);
        btn.textContent = favSet.has(restId) ? '❤️' : '♡';
        btn.onclick = (e) => handleFavoriteClick(e, restId);
    });
}

async function handleFavoriteClick(e, restaurantId) {
    e.stopPropagation();
    if (!isAuthenticated()) {
        alert('Войдите в аккаунт, чтобы добавлять в избранное');
        return;
    }
    const btn = e.currentTarget;
    const isFav = btn.textContent === '❤️';
    try {
        if (isFav) {
            await apiRequest(`/favourites/${restaurantId}`, 'DELETE', null, true);
            btn.textContent = '♡';
            showToast('Удалено из избранного');
        } else {
            await apiRequest(`/favourites/${restaurantId}`, 'POST', null, true);
            btn.textContent = '❤️';
            showToast('Добавлено в избранное');
        }
    } catch (err) {
        alert(err.message);
    }
}

async function loadFavouritesSet() {
    if (!isAuthenticated()) return new Set();
    try {
        const favs = await apiRequest('/favourites', 'GET', null, true);
        return new Set(favs.map(f => f.id));
    } catch(e) {
        return new Set();
    }
}

function showToast(message) {
    let toast = document.querySelector('.toast-message');
    if (!toast) {
        toast = document.createElement('div');
        toast.className = 'toast-message';
        toast.style.cssText = 'position:fixed;bottom:20px;right:20px;background:#333;color:#fff;padding:10px 20px;border-radius:5px;z-index:1000;display:none;';
        document.body.appendChild(toast);
    }
    toast.textContent = message;
    toast.style.display = 'block';
    setTimeout(() => { toast.style.display = 'none'; }, 2000);
}

function initCatalogFilters() {
    const searchInput = document.getElementById('search-input');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            currentFilters.search = e.target.value;
            loadRestaurants();
        });
    }
    const cuisineSelect = document.getElementById('cuisine-filter');
    if (cuisineSelect) {
        cuisineSelect.addEventListener('change', (e) => {
            currentFilters.cuisineType = e.target.value;
            loadRestaurants();
        });
    }
    const typeSelect = document.getElementById('type-filter');
    if (typeSelect) {
        typeSelect.addEventListener('change', (e) => {
            currentFilters.restaurantType = e.target.value;
            loadRestaurants();
        });
    }
    const priceSelect = document.getElementById('price-filter');
    if (priceSelect) {
        priceSelect.addEventListener('change', (e) => {
            currentFilters.priceRange = e.target.value;
            loadRestaurants();
        });
    }
    const ratingSelect = document.getElementById('rating-filter');
    if (ratingSelect) {
        ratingSelect.addEventListener('change', (e) => {
            const val = e.target.value;
            currentFilters.minRating = val ? parseFloat(val) : null;
            loadRestaurants();
        });
    }
}

if (document.getElementById('restaurants-list')) {
    initCatalogFilters();
    loadRestaurants();
}