const FAVORITES_KEY = 'verve_favorites';
const API_URL = 'http://localhost:8000';

export const favoritesService = {
    FAVORITES_KEY,

    getAll: () => {
        try {
            const favorites = localStorage.getItem(FAVORITES_KEY);
            if (favorites === '"[]"' || favorites === '[]') {
                localStorage.removeItem(FAVORITES_KEY);
                return [];
            }
            return favorites ? JSON.parse(favorites) : [];
        } catch (error) {
            console.error('Ошибка чтения избранного:', error);
            localStorage.removeItem(FAVORITES_KEY);
            return [];
        }
    },

    add: (establishmentId) => {
        const favorites = favoritesService.getAll();
        if (!favorites.includes(establishmentId)) {
            favorites.push(establishmentId);
            localStorage.setItem(FAVORITES_KEY, JSON.stringify(favorites));
            window.dispatchEvent(new StorageEvent('storage', {
                key: FAVORITES_KEY,
                newValue: JSON.stringify(favorites)
            }));
        }
        return favorites;
    },

    remove: (establishmentId) => {
        let favorites = favoritesService.getAll();
        favorites = favorites.filter(id => id !== establishmentId);
        localStorage.setItem(FAVORITES_KEY, JSON.stringify(favorites));
        window.dispatchEvent(new StorageEvent('storage', {
            key: FAVORITES_KEY,
            newValue: JSON.stringify(favorites)
        }));
        return favorites;
    },

    isFavorite: (establishmentId) => {
        const favorites = favoritesService.getAll();
        return favorites.includes(establishmentId);
    },

    clear: () => {
        localStorage.removeItem(FAVORITES_KEY);
        window.dispatchEvent(new StorageEvent('storage', {
            key: FAVORITES_KEY,
            newValue: null
        }));
    },

    getFavoritesDetails: async () => {
        console.log("🔄 Загрузка избранного...");

        const favoriteIds = favoritesService.getAll();
        console.log("📋 ID избранных:", favoriteIds);

        if (favoriteIds.length === 0) {
            console.log("📭 Нет избранных ID");
            return [];
        }

        try {
            console.log("🌐 Загружаем все рестораны...");
            const response = await fetch(`${API_URL}/establishments`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            const allRestaurants = data.establishments || [];
            console.log("✅ Всего ресторанов загружено:", allRestaurants.length);

            const favoriteRestaurants = allRestaurants.filter(r =>
                favoriteIds.includes(r.id)
            );

            console.log("🎯 Найдено избранных:", favoriteRestaurants.length);

            if (favoriteRestaurants.length === 0) {
                console.warn("⚠️ ID не найдены среди всех ресторанов");
                return [];
            }

            const getSafeImage = (images) => {
                try {

                    if (!images) {
                        return '/assets/images/venue.jpg';
                    }

                    if (Array.isArray(images)) {
                        const img = images[0];
                        return img ? normalizeImagePath(img) : '/assets/images/venue.jpg';
                    }


                    if (typeof images === 'string') {

                        if (images.trim().startsWith('[') || images.trim().startsWith('{')) {
                            try {
                                const parsed = JSON.parse(images);
                                if (Array.isArray(parsed) && parsed.length > 0) {
                                    return normalizeImagePath(parsed[0]);
                                } else if (typeof parsed === 'string') {
                                    return normalizeImagePath(parsed);
                                }
                            } catch (e) {

                            }
                        }
                        return normalizeImagePath(images);
                    }


                    return '/assets/images/venue.jpg';
                } catch (error) {
                    console.error("Ошибка обработки изображения:", error);
                    return '/assets/images/venue.jpg';
                }
            };

            const normalizeImagePath = (path) => {
                if (!path || path === '/assets/images/venue.jpg') {
                    return '/assets/images/venue.jpg';
                }

                let result = path.toString().trim();

                while (result.includes('/assets/assets/')) {
                    result = result.replace('/assets/assets/', '/assets/');
                }

                while (result.includes('images/images/')) {
                    result = result.replace('images/images/', 'images/');
                }

                if (result.startsWith('images/')) {
                    result = '/assets/' + result;
                }

                if (result.startsWith('/images/')) {
                    result = '/assets' + result;
                }


                if (!result.startsWith('/') && !result.startsWith('http')) {
                    result = '/assets/images/' + result;
                }

                return result || '/assets/images/venue.jpg';
            };

            const formattedData = favoriteRestaurants.map(est => {
                const image = getSafeImage(est.images);
                console.log(`Ресторан ${est.name}: изображение ${image}`);

                return {
                    id: est.id,
                    name: est.name || 'Без названия',
                    country: est.country?.name || est.country || 'Не указано',
                    city: est.city || 'Не указано',
                    image: image,
                    rating: est.rating || 4.0,
                    is_favorite: true
                };
            });

            console.log("✨ Возвращаем отформатированные данные");
            return formattedData;

        } catch (error) {
            console.error("💥 Ошибка загрузки:", error);
            return [];
        }
    }
};