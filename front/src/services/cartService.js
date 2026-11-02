import api from '../api/axios';

export const getCart = async () => {
  const res = await api.get('/cart');
  return res.data;
};

export const addToCart = async (tourId, quantity = 1) => {
  return api.post('/cart', { tourId, quantity });
};

export const removeFromCart = async (cartItemId) => {
  return api.delete(`/cart/${cartItemId}`);
};

export const clearCart = async () => {
  return api.delete('/cart/clear');
};

export const checkoutCart = async () => {
  return api.post('/cart/checkout');
};
