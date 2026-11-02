import api from '../api/axios';

export const getCountries = async () => {
  const res = await api.get('/countries');
  return res.data;
};

export const getCategories = async () => {
  const res = await api.get('/categories');
  return res.data;
};

export const getTourOperators = async () => {
  const res = await api.get('/tourOperators');
  return res.data;
};

export const getTags = async () => {
  const res = await api.get('/tags');
  return res.data;
};
