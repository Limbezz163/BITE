async function loadFavourites() {
    if (!isAuthenticated()) {
        window.location.href = 'auth.html';
        return;
    }
    const container = document.getElementById('favourites-list');
    try {
        const favourites = await apiRequest('/favourites', 'GET', null, true);
        if (!favourites.length) {
            container.innerHTML = '<div class="card">У вас пока нет избранных ресторанов. Перейдите в каталог, чтобы добавить.</div>';
            return;
        }
        container.innerHTML = favourites.map(r => `
            <div class="favorite-card card">
                <img src="${r.imageUrl || 'https://via.placeholder.com/300x200'}" alt="${r.name}" style="width:100%; height:200px; object-fit:cover;">
                <div class="favorite-content">
                    <div class="favorite-header">
                        <div>
                            <h3><a href="restaurant.html?id=${r.id}">${r.name}</a></h3>
                            <div class="rating">★ ${r.rating}</div>
                            <div class="price">${r.priceRange || '—'}</div>
                        </div>
                        <div class="favorite-actions">
                            <button class="remove-btn" data-id="${r.id}" title="Удалить из избранного">❌</button>
                        </div>
                    </div>
                    <p>${r.cuisineType || ''} • ${r.restaurantType || ''}</p>
                    <p>${r.shortDescription || (r.description ? r.description.substring(0,100)+'...' : '')}</p>
                </div>
            </div>
        `).join('');
        document.querySelectorAll('.remove-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.preventDefault();
                const restId = parseInt(btn.dataset.id);
                try {
                    await apiRequest(`/favourites/${restId}`, 'DELETE', null, true);
                    loadFavourites();
                } catch(err) { alert(err.message); }
            });
        });
    } catch(err) {
        console.error(err);
        container.innerHTML = '<div class="card">Ошибка загрузки избранного</div>';
    }
}

if (document.getElementById('favourites-list')) {
    loadFavourites();
}