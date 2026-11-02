import api from '../api/axios';

export const addToFavorites = async (tourId) => {
  await api.post('/favoriteTours', {
    tourId: tourId
  });
};

export const removeFromFavorites = async (tourId) => {
  await api.delete(`/favoriteTours/tour/${tourId}`);
};

export const getMyFavorites = async () => {
  const res = await api.get('/favoriteTours/my');
  return res.data;
};
