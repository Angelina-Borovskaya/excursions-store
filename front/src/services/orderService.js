import api from '../api/axios';

export const getMyOrders = async () => {
  const res = await api.get('/orders/my');
  return res.data;
};
