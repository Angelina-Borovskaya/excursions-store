import api from '../api/axios';

// GET
export const getCities = async () => {
  const res = await api.get('/cities');
  return res.data;
};

// POST
export const createCity = async ({ name, countryId }) => {
  const res = await api.post('/cities', {
    name,
    countryId
  });
  return res.data;
};

// PUT
export const updateCity = async ({ id, name, countryId }) => {
  await api.put(`/cities/${id}`, {
    id,
    name,
    countryId
  });
};

// DELETE
export const deleteCity = async (id) => {
  await api.delete(`/cities/${id}`);
};
