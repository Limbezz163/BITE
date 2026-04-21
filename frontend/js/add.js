document.addEventListener('DOMContentLoaded', function() {
    const submitBtn = document.getElementById('submit-restaurant');
    if (!submitBtn) {
        console.error('Кнопка не найдена');
        return;
    }
    
    submitBtn.addEventListener('click', async function(e) {
        e.preventDefault();
        
        if (!isAdmin()) {
            alert('Доступ только для администратора');
            return;
        }
        
        const openingDateRaw = document.getElementById('opening-date').value;
        let formattedDate = '';
        if (openingDateRaw) {
            const parts = openingDateRaw.split('.');
            if (parts.length === 3) {
                formattedDate = `${parts[2]}-${parts[1]}-${parts[0]}`;
            }
        }
        
        const formData = {
            Name: document.getElementById('restaurant-name').value,
            Description: document.getElementById('restaurant-description').value,
            CuisineType: document.getElementById('cuisine-type').value,
            RestaurantType: document.getElementById('restaurant-type').value,
            PriceRange: document.getElementById('price-range').value,
            Phone: document.getElementById('phone').value,
            City: document.getElementById('city').value,
            Street: document.getElementById('street').value,
            House: document.getElementById('house').value,
            OpeningDate: formattedDate,
            Features: [],
            ImageUrl: document.getElementById('image-url')?.value || ''
        };
        
        try {
            await apiRequest('/restaurants', 'POST', formData, true);
            alert('Ресторан успешно добавлен');
            window.location.href = 'admin.html';
        } catch(err) {
            console.error(err);
            alert('Ошибка: ' + err.message);
        }
    });
});