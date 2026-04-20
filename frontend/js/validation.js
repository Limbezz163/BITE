document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('.add-form');
    const errorContainer = document.createElement('div');
    errorContainer.className = 'error-container';
    form.insertBefore(errorContainer, form.firstChild);

    addHintElements();

    const validators = {
        'phone': validatePhone,
        'restaurant-name': validateName,
        'restaurant-Description': validateDescription,
        'city': validateCity,
        'street': validateStreet,
        'house': validateHouse,
        'opening-date': validateOpeningDate
    };

    Object.keys(validators).forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('blur', () => validateField(fieldId, validators[fieldId]));
            field.addEventListener('input', () => clearFieldError(fieldId));
        }
    });

    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        let isValid = true;
        const errors = [];

        Object.keys(validators).forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                const result = validators[fieldId](field.value);
                if (!result.isValid) {
                    isValid = false;
                    showFieldError(fieldId, result.message);
                    errors.push(`${getFieldLabel(fieldId)}: ${result.message}`);
                } else {
                    clearFieldError(fieldId);
                }
            }
        });

        if (!isValid) {
            showGeneralErrors(errors);
        } else {
            errorContainer.innerHTML = '';
            this.submit();
        }
    });

    function addHintElements() {
        const fieldsWithHints = {
            'phone': 'Формат: Только цифры и символ +, (, ), -',
            'restaurant-name': 'От 2 до 30 символов. Можно использовать только буквы (русские/английские) и цифры',
            'restaurant-Description': 'От 30 до 300 слов. Минимум 30 слов для подробного описания. Разрешены специльные символы и цифры',
            'city': 'От 2 до 20 букв. Можно использовать только криллицу и латинницу',
            'street': 'От 2 до 20 букв. Можно использовать только криллицу и латинницу',
            'house': 'От 1 до 4 символов. Можно использовать цифры и буквы',
            'opening-date': 'Дата в формате ДД.ММ.ГГГГ. Можно использовать только цифры и точки'
        };

        Object.keys(fieldsWithHints).forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                const hint = document.createElement('div');
                hint.className = 'field-hint';
                hint.textContent = fieldsWithHints[fieldId];
                field.parentNode.appendChild(hint);
                
                const errorElement = document.createElement('div');
                errorElement.className = 'field-error';
                errorElement.id = `${fieldId}-error`;
                field.parentNode.appendChild(errorElement);
            }
        });
    }

    function validateField(fieldId, validator) {
        const field = document.getElementById(fieldId);
        if (field) {
            const result = validator(field.value);
            if (!result.isValid) {
                showFieldError(fieldId, result.message);
            } else {
                clearFieldError(fieldId);
            }
        }
    }

    function showFieldError(fieldId, message) {
        const errorElement = document.getElementById(`${fieldId}-error`);
        const field = document.getElementById(fieldId);
        
        if (errorElement) {
            errorElement.textContent = message;
            field.classList.add('error');
        }
    }

    function clearFieldError(fieldId) {
        const errorElement = document.getElementById(`${fieldId}-error`);
        const field = document.getElementById(fieldId);
        
        if (errorElement) {
            errorElement.textContent = '';
            field.classList.remove('error');
        }
    }

    function showGeneralErrors(errors) {
        errorContainer.innerHTML = '';
        errorContainer.classList.add('error-container-visible');
        
        const errorList = document.createElement('ul');
        errorList.className = 'error-list';
        
        errors.forEach(error => {
            const listItem = document.createElement('li');
            listItem.textContent = error;
            errorList.appendChild(listItem);
        });
        
        errorContainer.appendChild(errorList);
    }

    function getFieldLabel(fieldId) {
        const labels = {
            'phone': 'Телефон',
            'restaurant-name': 'Название ресторана',
            'restaurant-Description': 'Описание ресторана',
            'city': 'Город',
            'street': 'Улица',
            'house': 'Дом',
            'opening-date': 'Дата открытия'
        };
        return labels[fieldId] || fieldId;
    }

    function validatePhone(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Телефон обязателен для заполнения' };
        }
        
        const phoneRegex = /^\+7\s?\(?\d{3}\)?\s?\d{3}-?\d{2}-?\d{2}$/;
        const cleanPhone = value.replace(/\D/g, '');
        
        if (!phoneRegex.test(value)) {
            return { isValid: false, message: 'Неверный формат телефона. Используйте: +7 (XXX) XXX-XX-XX. Можно использовать только цифры и символы +, (, ), -' };
        }
        
        if (cleanPhone.length !== 11 || !cleanPhone.startsWith('7')) {
            return { isValid: false, message: 'Телефон должен содержать 11 цифр (включая код страны)' };
        }
        
        return { isValid: true, message: '' };
    }

    function validateName(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Название обязательно для заполнения' };
        }
        
        if (value.length < 2) {
            return { isValid: false, message: 'Название должно содержать минимум 2 символа' };
        }
        
        if (value.length > 30) {
            return { isValid: false, message: 'Название должно содержать не более 30 символов' };
        }
        
        const nameRegex = /^[a-zA-Zа-яА-ЯёЁ0-9\s]+$/;
        if (!nameRegex.test(value)) {
            return { isValid: false, message: 'Название может содержать только буквы (русские/английские) и цифры. Специальные символы не допускаются' };
        }
        
        return { isValid: true, message: '' };
    }

    function validateDescription(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Описание обязательно для заполнения' };
        }
        
        const words = value.trim().split(/\s+/).filter(word => word.length > 0);
        
        if (words.length < 30) {
            return { isValid: false, message: `Описание должно содержать минимум 30 слов (сейчас: ${words.length})` };
        }
        
        if (words.length > 300) {
            return { isValid: false, message: `Описание должно содержать не более 300 слов (сейчас: ${words.length})` };
        }
        
        return { isValid: true, message: '' };
    }

    function validateCity(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Город обязателен для заполнения' };
        }
        
        if (value.length < 2) {
            return { isValid: false, message: 'Название города должно содержать минимум 2 буквы' };
        }
        
        if (value.length > 20) {
            return { isValid: false, message: 'Название города должно содержать не более 20 букв' };
        }
        
        const cityRegex = /^[a-zA-Zа-яА-ЯёЁ\s\-]+$/;
        if (!cityRegex.test(value)) {
            return { isValid: false, message: 'Название города может содержать только буквы (русские/английские), пробелы и дефисы. Цифры и другие символы не допускаются' };
        }
        
        return { isValid: true, message: '' };
    }

    function validateStreet(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Улица обязательна для заполнения' };
        }
        
        if (value.length < 2) {
            return { isValid: false, message: 'Название улицы должно содержать минимум 2 буквы' };
        }
        
        if (value.length > 20) {
            return { isValid: false, message: 'Название улицы должно содержать не более 20 букв' };
        }
        
        const streetRegex = /^[a-zA-Zа-яА-ЯёЁ\s\-]+$/;
        if (!streetRegex.test(value)) {
            return { isValid: false, message: 'Название улицы может содержать только буквы (русские/английские), пробелы и дефисы. Цифры и другие символы не допускаются' };
        }
        
        return { isValid: true, message: '' };
    }

    function validateHouse(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Номер дома обязателен для заполнения' };
        }
        
        if (value.length < 1) {
            return { isValid: false, message: 'Номер дома должен содержать минимум 1 символ' };
        }
        
        if (value.length > 4) {
            return { isValid: false, message: 'Номер дома должен содержать не более 4 символов' };
        }
        
        const houseRegex = /^[a-zA-Zа-яА-ЯёЁ0-9\/\-]+$/;
        if (!houseRegex.test(value)) {
            return { isValid: false, message: 'Номер дома может содержать только цифры, буквы (русские/английские), символы / и -' };
        }
        
        return { isValid: true, message: '' };
    }

    function validateOpeningDate(value) {
        if (!value.trim()) {
            return { isValid: false, message: 'Дата открытия обязательна для заполнения' };
        }
        
        const dateRegex = /^\d{2}\.\d{2}\.\d{4}$/;
        if (!dateRegex.test(value)) {
            return { isValid: false, message: 'Неверный формат даты. Используйте ДД.ММ.ГГГГ. Можно использовать только цифры и точки' };
        }
        
        const parts = value.split('.');
        const day = parseInt(parts[0]);
        const month = parseInt(parts[1]);
        const year = parseInt(parts[2]);
        
        const date = new Date(year, month - 1, day);
        if (date.getDate() !== day || date.getMonth() !== month - 1 || date.getFullYear() !== year) {
            return { isValid: false, message: 'Некорректная дата. Проверьте правильность ввода дня, месяца и года' };
        }
        
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        if (date > today) {
            return { isValid: false, message: 'Дата открытия не может быть в будущем' };
        }
        
        const hundredYearsAgo = new Date();
        hundredYearsAgo.setFullYear(today.getFullYear() - 100);
        
        if (date < hundredYearsAgo) {
            return { isValid: false, message: 'Дата открытия не может быть старше 100 лет' };
        }
        
        return { isValid: true, message: '' };
    }
});