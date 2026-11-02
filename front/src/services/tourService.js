import api from '../api/axios';

export const getTours = (params) =>
  api.get('/tours', { params }).then(res => res.data);

export const getTourById = (id) =>
  api.get(`/tours/${id}`).then(res => res.data);

// ADMIN: multipart/form-data
const toTourFormData = (payload) => {
  const fd = new FormData();

  fd.append('title', payload.title ?? '');
  fd.append('description', payload.description ?? '');
  fd.append('price', String(payload.price ?? 0));
  fd.append('startDate', payload.startDate); // 'YYYY-MM-DD'
  fd.append('endDate', payload.endDate);

  fd.append('cityId', String(payload.cityId));
  fd.append('categoryId', String(payload.categoryId));
  fd.append('tourOperatorId', String(payload.tourOperatorId));

  // image
  if (payload.imageFile) fd.append('image', payload.imageFile);

  // tagIds: повторяющиеся ключи или индексированные — оба варианта обычно ок.
  (payload.tagIds ?? []).forEach(id => fd.append('tagIds', String(id)));

  return fd;
};

export const createTour = (payload) =>
  api.post('/tours', toTourFormData(payload), {
    headers: { 'Content-Type': 'multipart/form-data' }
  }).then(r => r.data);

export const updateTour = (id, payload) =>
  api.put(`/tours/${id}`, toTourFormData(payload), {
    headers: { 'Content-Type': 'multipart/form-data' }
  });

export const deleteTour = (id) =>
  api.delete(`/tours/${id}`);
