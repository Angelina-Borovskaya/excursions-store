import React, { useEffect, useState } from 'react';
import ExcursionCard from '../../components/Card/ExcursionCard';
import Filter from '../../components/Filter/Filter';
import { getTours } from '../../services/tourService';
import {
  getCountries,
  getCategories,
  getTourOperators,
  getTags
} from '../../services/filterService';
import { useFavorites } from '../../context/FavoritesContext';
import { useCart } from '../../context/CartContext';
import './CatalogPage.css';
import { useAuthAction } from '../../auth/useAuthAction';


const CatalogPage = () => {
  const [filters, setFilters] = useState({});
  const [filterOptions, setFilterOptions] = useState(null);
  const { cartIds, toggleCart } = useCart();
  const authAction = useAuthAction();


  const [excursions, setExcursions] = useState([]);
  const [loading, setLoading] = useState(true);

  // ⭐ ИЗБРАННОЕ ИЗ КОНТЕКСТА
  const { isFavorite, toggleFavorite } = useFavorites();

  // 1️⃣ Загружаем фильтры и первый список туров
  useEffect(() => {
    loadFilters();
    loadTours({});
  }, []);

  // 2️⃣ Перезагружаем туры при изменении фильтров
  useEffect(() => {
    if (!filterOptions) return;
    loadTours(filters);
  }, [filters, filterOptions]);

  const loadFilters = async () => {
    try {
      const [countries, categories, operators, tags] = await Promise.all([
        getCountries(),
        getCategories(),
        getTourOperators(),
        getTags()
      ]);

      setFilterOptions({
        Рейтинг: [5, 4, 3, 2, 1],
        Страна: countries,
        Категория: categories,
        Туроператор: operators,
        Тэги: tags
      });
    } catch (e) {
      console.error('Ошибка загрузки фильтров', e);
    }
  };

  const loadTours = async (activeFilters = {}) => {
    try {
      setLoading(true);

      const params = {};

      if (activeFilters['Страна']?.size)
        params.countryId = [...activeFilters['Страна']];

      if (activeFilters['Категория']?.size)
        params.categoryId = [...activeFilters['Категория']];

      if (activeFilters['Туроператор']?.size)
        params.tourOperatorId = [...activeFilters['Туроператор']];

      if (activeFilters['Тэги']?.size)
        params.tagId = [...activeFilters['Тэги']];

      if (activeFilters['Рейтинг']?.size)
        params.minRating = Math.min(...activeFilters['Рейтинг']);

      const data = await getTours(params);
      setExcursions(data);
    } catch (e) {
      console.error('Ошибка загрузки туров', e);
    } finally {
      setLoading(false);
    }
  };

  if (!filterOptions || loading) {
    return <div className="catalog-loading">Загрузка...</div>;
  }

  return (
    <div className="catalog-wrapper">
      <div className="layout-catalog">
        <Filter filters={filterOptions} onChange={setFilters} />

        <main className="content-catalog">
          <div className="catalog-grid">
            {excursions.map(excursion => (
              <ExcursionCard
                key={excursion.id}
                excursion={excursion}
                isFavorite={isFavorite(excursion.id)}
                onToggleFavorite={(id) =>
                  authAction(() => toggleFavorite(id))
                }
                inCart={cartIds.has(excursion.id)}
                onToggleCart={(id) =>
                  authAction(() => toggleCart(id))
                }
              />
            ))}
          </div>

          {excursions.length === 0 && (
            <div className="no-results">Ничего не найдено</div>
          )}
        </main>
      </div>
    </div>
  );
};

export default CatalogPage;
