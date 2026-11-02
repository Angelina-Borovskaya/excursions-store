import api from '../api/axios';

export const getTags = async () => {
  const res = await api.get('/tags');
  return res.data;
};

export const createTag = async (name) => {
  const res = await api.post('/tags', { name });
  return res.data;
};

export const updateTag = async (id, name) => {
  await api.put(`/tags/${id}`, { id, name });
};

export const deleteTag = async (id) => {
  await api.delete(`/tags/${id}`);
};
