import api from '../api/axios';

// GET
export const getTourOperators = async () => {
  const res = await api.get('/touroperators');
  return res.data;
};

// POST
export const createTourOperator = async (name) => {
  const res = await api.post('/touroperators', { name });
  return res.data;
};

// PUT
export const updateTourOperator = async (id, name) => {
  await api.put(`/touroperators/${id}`, {
    id,
    name
  });
};

// DELETE
export const deleteTourOperator = async (id) => {
  await api.delete(`/touroperators/${id}`);
};
