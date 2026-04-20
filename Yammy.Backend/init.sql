SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;
CREATE DATABASE IF NOT EXISTS yammy_db;
USE yammy_db;

CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    role ENUM('user', 'admin') DEFAULT 'user',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    avatar_color VARCHAR(7) DEFAULT '#3498db'
);

CREATE TABLE IF NOT EXISTS restaurants (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    short_description VARCHAR(200),
    cuisine_type VARCHAR(50),
    restaurant_type VARCHAR(50),
    price_range VARCHAR(10),
    phone VARCHAR(20),
    address VARCHAR(255),
    city VARCHAR(50),
    street VARCHAR(100),
    house VARCHAR(20),
    opening_date DATE,
    rating DECIMAL(3,2) DEFAULT 0,
    image_url VARCHAR(255),
    features TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS reviews (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    restaurant_id INT NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_restaurant (user_id, restaurant_id)
);

CREATE TABLE IF NOT EXISTS favourites (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    restaurant_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_restaurant_fav (user_id, restaurant_id)
);

-- Пользователи: админ + 5 обычных
INSERT INTO users (full_name, email, password_hash, phone, role) VALUES
('Admin', 'admin@yammy.com', '$2a$11$K1qQzQzQzQzQzQzQzQzQzQ', '+7 (999) 000-00-00', 'admin'),
('Иван Иванов', 'ivan@yammy.com', '$2a$11$K1qQzQzQzQzQzQzQzQzQzQ', '+7 (999) 123-45-67', 'user'),
('Анна Петрова', 'anna@yammy.com', '$2a$11$K1qQzQzQzQzQzQzQzQzQzQ', '+7 (999) 234-56-78', 'user'),
('Дмитрий Смирнов', 'dmitry@yammy.com', '$2a$11$K1qQzQzQzQzQzQzQzQzQzQ', '+7 (999) 345-67-89', 'user'),
('Елена Козлова', 'elena@yammy.com', '$2a$11$K1qQzQzQzQzQzQzQzQzQzQ', '+7 (999) 456-78-90', 'user'),
('Павел Новиков', 'pavel@yammy.com', '$2a$11$K1qQzQzQzQzQzQzQzQzQzQ', '+7 (999) 567-89-01', 'user');

-- 12 ресторанов с правильными кухнями (Итальянская, Грузинская, Японская, Американская, Русская)
-- и типами (Ресторан, Кафе, Бар, Кофейня)
INSERT INTO restaurants (name, description, short_description, cuisine_type, restaurant_type, price_range, phone, address, city, street, house, rating, image_url) VALUES
('La Piazza', 'Уютный итальянский ресторан. Паста, пицца из дровяной печи, домашнее вино.', 'Аутентичная Италия', 'Итальянская', 'Ресторан', '$$$', '+7 (495) 123-45-67', 'ул. Тверская, д. 15', 'Москва', 'Тверская', '15', 4.8, 'https://i.pinimg.com/1200x/8e/89/4d/8e894d83499a5569e4229d5ec77bfb16.jpg'),
('Sakura', 'Японский ресторан с суши-баром. Свежайшая рыба, авторские роллы.', 'Изысканная Япония', 'Японская', 'Ресторан', '$$$$', '+7 (495) 234-56-78', 'ул. Арбат, д. 10', 'Москва', 'Арбат', '10', 4.6, 'https://i.pinimg.com/736x/59/d6/ce/59d6cee515a0db4e785e2ee66c274691.jpg'),
('Грузия', 'Грузинский дворик с живой музыкой. Хинкали, хачапури, сациви.', 'Грузинское гостеприимство', 'Грузинская', 'Ресторан', '$$', '+7 (495) 345-67-89', 'ул. Пятницкая, д. 25', 'Москва', 'Пятницкая', '25', 4.9, 'https://i.pinimg.com/1200x/66/73/9d/66739db7bc1de1363330634fefe44299.jpg'),
('Burger House', 'Бургерная с крафтовым пивом. Авторские бургеры, картошка фри.', 'Лучшие бургеры', 'Американская', 'Бар', '$', '+7 (495) 456-78-90', 'ул. Новый Арбат, д. 8', 'Москва', 'Новый Арбат', '8', 4.3, 'https://i.pinimg.com/736x/d5/4c/54/d54c5487914164ffdc2d6f7fff51d00a.jpg'),
('Русская усадьба', 'Традиционная русская кухня: пельмени, блины, щи, сбитень.', 'Русские традиции', 'Русская', 'Ресторан', '$$', '+7 (495) 567-89-01', 'ул. Остоженка, д. 12', 'Москва', 'Остоженка', '12', 4.7, 'https://i.pinimg.com/736x/59/27/fc/5927fc3b7078ba0b318ec097242adc32.jpg'),
('Steak House', 'Стейки из мраморной говядины, гриль-меню, авторские соусы.', 'Премиальные стейки', 'Американская', 'Ресторан', '$$$$$', '+7 (495) 789-01-23', 'ул. Большая Дмитровка, д. 20', 'Москва', 'Большая Дмитровка', '20', 4.8, 'https://i.pinimg.com/736x/e0/6f/ce/e06fcecc0ecd82dc21e2954192228b5a.jpg'),
('Mama Mia', 'Семейный итальянский ресторан с детской комнатой. Паста, пицца.', 'Для всей семьи', 'Итальянская', 'Кафе', '$$', '+7 (495) 333-44-55', 'ул. Профсоюзная, д. 100', 'Москва', 'Профсоюзная', '100', 4.5, 'https://i.pinimg.com/736x/64/90/e7/6490e7a36c28484217a29e924e7733c5.jpg'),
('Суши-маркет', 'Сеть японских ресторанов. Суши, роллы, доставка.', 'Быстро и вкусно', 'Японская', 'Кафе', '$$', '+7 (495) 222-55-77', 'ул. Дмитровское шоссе, д. 35', 'Москва', 'Дмитровское шоссе', '35', 4.2, 'https://i.pinimg.com/736x/46/14/f5/4614f5effce2cc19d8afc1cc73f63703.jpg'),
('Кофейня «Уют»', 'Ароматный кофе, круассаны, десерты. Работает до полуночи.', 'Кофе и выпечка', 'Русская', 'Кофейня', '$', '+7 (495) 678-90-12', 'ул. Покровка, д. 5', 'Москва', 'Покровка', '5', 4.5, 'https://i.pinimg.com/1200x/d9/52/a0/d952a0cda665f10bd115a7503e416d98.jpg'),
('Пельменная №8', 'Домашние пельмени, вареники, супы. Быстро, вкусно, недорого.', 'Домашняя кухня', 'Русская', 'Кафе', '$', '+7 (495) 333-66-99', 'ул. Арбат, д. 23', 'Москва', 'Арбат', '23', 4.5, 'https://i.pinimg.com/736x/59/a4/f6/59a4f6b05d4d4fbe0973e801822662ef.jpg'),
('Кофе и круассаны', 'Уютная кондитерская. Свежая выпечка, зерновой кофе.', 'Для сладкоежек', 'Русская', 'Кофейня', '$', '+7 (495) 444-77-00', 'ул. Тверская, д. 3', 'Москва', 'Тверская', '3', 4.8, 'https://i.pinimg.com/736x/a2/a4/c1/a2a4c19b1b61433c59e722111f91b904.jpg'),
('Гастробар №1', 'Авторская кухня, крафтовое пиво, коктейли. Живая музыка по пятницам.', 'Современный гастробар', 'Русская', 'Бар', '$$$', '+7 (495) 111-22-44', 'ул. Ленина, д. 10', 'Москва', 'Ленина', '10', 4.7, 'https://i.pinimg.com/736x/a9/85/a9/a985a9fec8a9f8ed27839165884a6040.jpg');

-- Отзывы: на первый ресторан (La Piazza) – 6 отзывов, на остальные – по 1
INSERT INTO reviews (user_id, restaurant_id, rating, comment) VALUES
-- La Piazza (id=1)
(2,1,5,'Итальянская кухня на высоте! Паста – божественна.'),
(3,1,4,'Очень атмосферно, но немного шумно.'),
(4,1,5,'Обслуживание отличное, вино рекомендовали прекрасное.'),
(5,1,5,'Лучший итальянский ресторан в городе.'),
(6,1,4,'Цены кусаются, но качество того стоит.'),
(2,1,5,'Был дважды – оба раза всё идеально.'),
-- Sakura (id=2)
(3,2,5,'Свежие суши, отличный выбор сашими.'),
-- Грузия (id=3)
(4,3,5,'Хачапури по-аджарски – просто бомба!'),
-- Burger House (id=4)
(5,4,5,'Лучший бургер в городе.'),
-- Русская усадьба (id=5)
(6,5,5,'Щи и пельмени – как у бабушки.'),
-- Steak House (id=6)
(2,6,5,'Стейк рибай – пальчики оближешь.'),
-- Mama Mia (id=7)
(3,7,5,'Пицца Маргарита – как в Неаполе.'),
-- Суши-маркет (id=8)
(4,8,4,'Суши свежие, но роллы простоваты.'),
-- Кофейня «Уют» (id=9)
(5,9,5,'Латте и круассан – отличное начало дня.'),
-- Пельменная №8 (id=10)
(6,10,5,'Пельмени с бульоном – как дома.'),
-- Кофе и круассаны (id=11)
(2,11,5,'Круассаны с миндалём – объедение.'),
-- Гастробар №1 (id=12)
(3,12,5,'Коктейли – огонь, кухня удивляет.');

-- Избранное (для демонстрации)
INSERT INTO favourites (user_id, restaurant_id) VALUES
(2,1), (2,4), (2,6), (3,1), (3,3), (3,5), (4,2), (4,8), (5,10), (6,12);