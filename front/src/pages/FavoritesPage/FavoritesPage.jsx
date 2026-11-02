import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import ExcursionCard from '../../components/Card/ExcursionCard';
import { useFavorites } from '../../context/FavoritesContext';
import { getTourById } from '../../services/tourService';
import './FavoritesPage.css';

const FavoritesPage = () => {
  const { favoriteIds, toggleFavorite, loading } = useFavorites();
  const [tours, setTours] = useState([]);
  const [loadingTours, setLoadingTours] = useState(true);

  useEffect(() => {
    const loadTours = async () => {
      if (favoriteIds.size === 0) {
        setTours([]);
        setLoadingTours(false);
        return;
      }

      try {
        const data = await Promise.all(
          Array.from(favoriteIds).map(id => getTourById(id))
        );
        setTours(data);
      } catch (e) {
        console.error('Ошибка загрузки туров избранного', e);
      } finally {
        setLoadingTours(false);
      }
    };

    loadTours();
  }, [favoriteIds]);

  if (loading || loadingTours) return <div>Загрузка...</div>;

  return (
    <div className="favorites-page">
      <div className="favorites-container">
        <h1 className="favorites-title">Избранное</h1>

        {tours.length === 0 ? (
          <div className="favorites-empty">
            <img src="/assets/icons/heart2.svg" alt="Пусто" />
            <h2>В избранном пока пусто</h2>
            <Link to="/catalog">Перейти в каталог</Link>
          </div>
        ) : (
          <div className="favorites-grid">
            {tours.map(tour => (
              <ExcursionCard
                key={tour.id}
                excursion={tour}
                isFavorite
                onToggleFavorite={() => toggleFavorite(tour.id)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default FavoritesPage;
