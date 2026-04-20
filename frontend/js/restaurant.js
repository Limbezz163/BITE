let currentRestaurantId = null;
let selectedRating = 0;

async function loadRestaurantInfo() {
    const urlParams = new URLSearchParams(window.location.search);
    const id = urlParams.get('id');
    if (!id) { window.location.href = 'catalog.html'; return; }
    currentRestaurantId = parseInt(id);
    try {
        const restaurant = await apiRequest(`/restaurants/${currentRestaurantId}`);
        document.getElementById('restaurant-name').textContent = restaurant.name;
        document.getElementById('restaurant-cuisine').textContent = restaurant.cuisineType || '—';
        document.getElementById('restaurant-rating').innerHTML = `★ ${restaurant.rating}`;
        document.getElementById('restaurant-type').textContent = restaurant.restaurantType || '—';
        document.getElementById('restaurant-price').textContent = restaurant.priceRange || '—';
        document.getElementById('restaurant-description').textContent = restaurant.description || '';
        document.getElementById('restaurant-address').textContent = 
            `${restaurant.city || ''} ${restaurant.street || ''} ${restaurant.house || ''}`.trim() || 'Адрес не указан';
        const carouselImg = document.getElementById('carousel-image');
        if (carouselImg) carouselImg.src = restaurant.imageUrl || 'https://via.placeholder.com/800x400?text=No+Image';
        const favBtn = document.getElementById('favorite-btn');
        if (favBtn && isAuthenticated()) {
            const isFav = await apiRequest(`/favourites/check/${currentRestaurantId}`, 'GET', null, true);
            favBtn.textContent = isFav.isFavourite ? '❤️' : '♡';
            favBtn.style.display = 'inline-block';
            favBtn.onclick = () => toggleFavorite();
        }
    } catch(err) {
        console.error(err);
        alert('Ошибка загрузки ресторана');
    }
}

async function toggleFavorite() {
    if (!isAuthenticated()) { alert('Войдите в аккаунт'); return; }
    const btn = document.getElementById('favorite-btn');
    const isFav = btn.textContent === '❤️';
    try {
        if (isFav) {
            await apiRequest(`/favourites/${currentRestaurantId}`, 'DELETE', null, true);
            btn.textContent = '♡';
        } else {
            await apiRequest(`/favourites/${currentRestaurantId}`, 'POST', null, true);
            btn.textContent = '❤️';
        }
    } catch(err) { alert(err.message); }
}

async function loadReviews(reset = true) {
    try {
        const url = `/reviews/restaurant/${currentRestaurantId}`;
        const newReviews = await apiRequest(url);
        const container = document.getElementById('reviews-container');
        if (newReviews && newReviews.length) {
            renderReviews(newReviews);
            const showMoreBtn = document.getElementById('show-more-reviews');
            if (showMoreBtn) showMoreBtn.style.display = 'none';
        } else {
            if (container) container.innerHTML = '<div class="card">Нет отзывов. Будьте первым!</div>';
            const showMoreBtn = document.getElementById('show-more-reviews');
            if (showMoreBtn) showMoreBtn.style.display = 'none';
        }
    } catch(err) { console.error(err); }
}

function renderReviews(reviews) {
    const container = document.getElementById('reviews-container');
    if (!container) return;
    container.innerHTML = reviews.map(rev => `
        <div class="review-item card">
            <div class="review-header">
                <div class="review-author">${rev.user?.fullName || 'Аноним'}</div>
                <div class="review-rating">★ ${rev.rating}</div>
            </div>
            <p>${rev.comment || ''}</p>
            <small>${new Date(rev.createdAt).toLocaleDateString()}</small>
        </div>
    `).join('');
}

function initStarRating() {
    const stars = document.querySelectorAll('.star');
    
    function setRating(rating) {
        selectedRating = rating;
        stars.forEach((star, index) => {
            if (index < rating) {
                star.classList.add('selected');
            } else {
                star.classList.remove('selected');
            }
        });
        const addBtn = document.getElementById('add-review-btn');
        const reviewText = document.getElementById('review-text').value.trim();
        if (addBtn) addBtn.disabled = !(reviewText && selectedRating > 0);
    }

    stars.forEach((star, index) => {
        star.addEventListener('mouseenter', () => {
            stars.forEach((s, i) => {
                if (i <= index) {
                    s.classList.add('hover');
                } else {
                    s.classList.remove('hover');
                }
            });
        });
        star.addEventListener('mouseleave', () => {
            stars.forEach(s => s.classList.remove('hover'));
            stars.forEach((s, i) => {
                if (i < selectedRating) {
                    s.classList.add('selected');
                } else {
                    s.classList.remove('selected');
                }
            });
        });
        star.addEventListener('click', () => {
            setRating(index + 1);
        });
    });

    const reviewTextElem = document.getElementById('review-text');
    if (reviewTextElem) {
        reviewTextElem.addEventListener('input', () => {
            const addBtn = document.getElementById('add-review-btn');
            if (addBtn) addBtn.disabled = !(reviewTextElem.value.trim() && selectedRating > 0);
        });
    }
}

async function submitReview() {
    if (selectedRating === 0) {
        alert('Поставьте оценку');
        return;
    }
    const comment = document.getElementById('review-text').value.trim();
    if (!comment) {
        alert('Напишите текст отзыва');
        return;
    }
    if (!isAuthenticated()) {
        alert('Войдите в аккаунт, чтобы оставить отзыв');
        return;
    }
    try {
        await apiRequest('/reviews', 'POST', {
            restaurantId: currentRestaurantId,
            rating: selectedRating,
            comment: comment
        }, true);
        alert('Отзыв добавлен');
        document.getElementById('review-text').value = '';
        selectedRating = 0;
        document.querySelectorAll('.star').forEach(s => s.classList.remove('selected'));
        const addBtn = document.getElementById('add-review-btn');
        if (addBtn) addBtn.disabled = true;
        loadReviews(true);
    } catch(err) {
        alert(err.message);
    }
}

if (document.getElementById('restaurant-name')) {
    initStarRating();
    loadRestaurantInfo();
    loadReviews(true);
    const submitBtn = document.getElementById('add-review-btn');
    if (submitBtn) submitBtn.onclick = submitReview;
}