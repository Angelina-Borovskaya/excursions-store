import api from '../api/axios';

// GET
export const getCountries = async () => {
  const res = await api.get('/countries');
  return res.data;
};

// POST
export const createCountry = async (name) => {
  const res = await api.post('/countries', { name });
  return res.data;
};

// PUT
export const updateCountry = async (id, name) => {
  await api.put(`/countries/${id}`, {
    id,
    name
  });
};

// DELETE
export const deleteCountry = async (id) => {
  await api.delete(`/countries/${id}`);
};
