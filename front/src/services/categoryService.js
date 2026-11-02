import api from '../api/axios';

// получить все категории
export const getCategories = async () => {
  const res = await api.get('/categories');
  return res.data;
};

// создать категорию
export const createCategory = async (name) => {
  const res = await api.post('/categories', { name });
  return res.data;
};

// обновить категорию
export const updateCategory = async (id, name) => {
  await api.put(`/categories/${id}`, {
    id,
    name
  });
};

// удалить категорию
export const deleteCategory = async (id) => {
  await api.delete(`/categories/${id}`);
};
