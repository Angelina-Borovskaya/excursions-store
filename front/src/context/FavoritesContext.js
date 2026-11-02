import { createContext, useContext, useEffect, useState } from 'react';
import {
  getMyFavorites,
  addToFavorites,
  removeFromFavorites
} from '../services/favoriteService';

const FavoritesContext = createContext();

export const FavoritesProvider = ({ children }) => {
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadFavorites();
  }, []);

  const loadFavorites = async () => {
    const token = localStorage.getItem('token');
    if (!token) {
      setLoading(false);
      return;
    }

    try {
      const data = await getMyFavorites();
      // data = [{ id, tour: { id, ... } }]
      setFavoriteIds(new Set(data.map(f => f.tour.id)));
    } catch (e) {
      console.error('Ошибка загрузки избранного', e);
    } finally {
      setLoading(false);
    }
  };

  const toggleFavorite = async (tourId) => {
    if (favoriteIds.has(tourId)) {
      await removeFromFavorites(tourId);
      setFavoriteIds(prev => {
        const s = new Set(prev);
        s.delete(tourId);
        return s;
      });
    } else {
      await addToFavorites(tourId);
      setFavoriteIds(prev => new Set(prev).add(tourId));
    }
  };

  const isFavorite = (tourId) => favoriteIds.has(tourId);

  return (
    <FavoritesContext.Provider
      value={{
        favoriteIds,
        isFavorite,
        toggleFavorite,
        loading
      }}
    >
      {children}
    </FavoritesContext.Provider>
  );
};

export const useFavorites = () => useContext(FavoritesContext);
