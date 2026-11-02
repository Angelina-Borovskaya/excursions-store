import { useCallback, useEffect, useState } from 'react';
import './Profile.css';
import ReportModal from '../../components/Reports/ReportModal';


import {
  getCountries, createCountry, updateCountry, deleteCountry
} from '../../services/countryService';

import {
  getCities, createCity, updateCity, deleteCity
} from '../../services/cityService';

import {
  getCategories, createCategory, updateCategory, deleteCategory
} from '../../services/categoryService';

import {
  getTourOperators, createTourOperator, updateTourOperator, deleteTourOperator
} from '../../services/tourOperatorService';

import {
  getTags, createTag, updateTag, deleteTag
} from '../../services/tagService';

import {
  getTours, createTour, updateTour, deleteTour
} from '../../services/tourService';

const AdminPanel = () => {
  /* ===== DATA ===== */
  const [countries, setCountries] = useState([]);
  const [cities, setCities] = useState([]);
  const [categories, setCategories] = useState([]);
  const [operators, setOperators] = useState([]);
  const [tags, setTags] = useState([]);
  const [tours, setTours] = useState([]);

  /* ===== FORMS: справочники ===== */
  const [newCountry, setNewCountry] = useState('');
  const [newCity, setNewCity] = useState('');
  const [cityCountryId, setCityCountryId] = useState('');
  const [newCategory, setNewCategory] = useState('');
  const [newOperator, setNewOperator] = useState('');
  const [newTag, setNewTag] = useState('');

  /* ===== TOURS FORM ===== */
  const [editingTourId, setEditingTourId] = useState(null);

  const [tourForm, setTourForm] = useState({
    title: '',
    description: '',
    price: '',
    startDate: '',
    endDate: '',
    cityId: '',
    categoryId: '',
    tourOperatorId: '',
    tagIds: [],
    imageFile: null
  });

  const handleTourChange = (field, value) => {
    setTourForm(prev => ({ ...prev, [field]: value }));
  };

  const toggleTourTag = (tagId) => {
    setTourForm(prev => {
      const set = new Set(prev.tagIds);
      if (set.has(tagId)) set.delete(tagId);
      else set.add(tagId);
      return { ...prev, tagIds: Array.from(set) };
    });
  };

  const resetTourForm = () => {
    setEditingTourId(null);
    setTourForm({
      title: '',
      description: '',
      price: '',
      startDate: '',
      endDate: '',
      cityId: '',
      categoryId: '',
      tourOperatorId: '',
      tagIds: [],
      imageFile: null
    });
  };

  
  const [isReportOpen, setIsReportOpen] = useState(false);

  /* ===== LOADERS ===== */
  const loadCountries = useCallback(async () => setCountries(await getCountries()), []);
  const loadCities = useCallback(async () => setCities(await getCities()), []);
  const loadCategories = useCallback(async () => setCategories(await getCategories()), []);
  const loadOperators = useCallback(async () => setOperators(await getTourOperators()), []);
  const loadTags = useCallback(async () => setTags(await getTags()), []);
  const loadTours = useCallback(async () => setTours(await getTours()), []);

  const loadAll = useCallback(async () => {
    await Promise.all([
      loadCountries(),
      loadCities(),
      loadCategories(),
      loadOperators(),
      loadTags(),
      loadTours()
    ]);
  }, [loadCountries, loadCities, loadCategories, loadOperators, loadTags, loadTours]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  /* ===== справочники: handlers ===== */
  const onAddCountry = async () => {
    if (!newCountry.trim()) return;
    await createCountry(newCountry.trim());
    setNewCountry('');
    loadCountries();
  };

  const onEditCountry = async (c) => {
    const name = prompt('Новое название страны', c.name);
    if (!name || name.trim() === c.name) return;
    await updateCountry(c.id, name.trim());
    loadCountries();
  };

  const onDeleteCountry = async (id) => {
    if (!window.confirm('Удалить страну?')) return;
    await deleteCountry(id);
    loadCountries();
    loadCities(); // города могли зависеть
  };

  const onAddCity = async () => {
    if (!newCity.trim() || !cityCountryId) return;
    await createCity({ name: newCity.trim(), countryId: Number(cityCountryId) });
    setNewCity('');
    setCityCountryId('');
    loadCities();
  };

  const onEditCity = async (city) => {
    const name = prompt('Новое название города', city.name);
    if (!name || name.trim() === city.name) return;
    await updateCity(city.id, { name: name.trim(), countryId: city.countryId ?? city.country?.id });
    loadCities();
  };

  const onDeleteCity = async (id) => {
    if (!window.confirm('Удалить город?')) return;
    await deleteCity(id);
    loadCities();
  };

  const onAddCategory = async () => {
    if (!newCategory.trim()) return;
    await createCategory(newCategory.trim());
    setNewCategory('');
    loadCategories();
  };

  const onEditCategory = async (cat) => {
    const name = prompt('Новое название категории', cat.name);
    if (!name || name.trim() === cat.name) return;
    await updateCategory(cat.id, name.trim());
    loadCategories();
  };

  const onDeleteCategory = async (id) => {
    if (!window.confirm('Удалить категорию?')) return;
    await deleteCategory(id);
    loadCategories();
  };

  const onAddOperator = async () => {
    if (!newOperator.trim()) return;
    await createTourOperator(newOperator.trim());
    setNewOperator('');
    loadOperators();
  };

  const onEditOperator = async (op) => {
    const name = prompt('Новое название туроператора', op.name);
    if (!name || name.trim() === op.name) return;
    await updateTourOperator(op.id, name.trim());
    loadOperators();
  };

  const onDeleteOperator = async (id) => {
    if (!window.confirm('Удалить туроператора?')) return;
    await deleteTourOperator(id);
    loadOperators();
  };

  const onAddTag = async () => {
    if (!newTag.trim()) return;
    await createTag(newTag.trim());
    setNewTag('');
    loadTags();
  };

  const onEditTag = async (tag) => {
    const name = prompt('Новое название тега', tag.name);
    if (!name || name.trim() === tag.name) return;
    await updateTag(tag.id, name.trim());
    loadTags();
  };

  const onDeleteTag = async (id) => {
    if (!window.confirm('Удалить тег?')) return;
    await deleteTag(id);
    loadTags();
  };

  /* ===== TOURS: handlers ===== */
  const onSaveTour = async () => {
    // базовая валидация
    if (!tourForm.title.trim()) return alert('Введите название');
    if (!tourForm.price) return alert('Введите цену');
    if (!tourForm.startDate || !tourForm.endDate) return alert('Введите даты');
    if (!tourForm.cityId || !tourForm.categoryId || !tourForm.tourOperatorId) {
      return alert('Выберите город/категорию/оператора');
    }

    const payload = {
      title: tourForm.title.trim(),
      description: tourForm.description,
      price: Number(tourForm.price),
      startDate: tourForm.startDate,
      endDate: tourForm.endDate,
      cityId: Number(tourForm.cityId),
      categoryId: Number(tourForm.categoryId),
      tourOperatorId: Number(tourForm.tourOperatorId),
      tagIds: tourForm.tagIds.map(Number),
      imageFile: tourForm.imageFile
    };

    try {
      if (editingTourId) {
        await updateTour(editingTourId, payload);
        alert('Экскурсия обновлена');
      } else {
        await createTour(payload);
        alert('Экскурсия создана');
      }

      resetTourForm();
      loadTours();
    } catch (e) {
      console.error(e);
      alert('Ошибка сохранения экскурсии');
    }
  };

  const onEditTour = (t) => {
    setEditingTourId(t.id);
    setTourForm({
      title: t.title ?? '',
      description: t.description ?? '',
      price: String(t.price ?? ''),
      startDate: t.startDate ? t.startDate.slice(0, 10) : '',
      endDate: t.endDate ? t.endDate.slice(0, 10) : '',
      cityId: String(t.city?.id ?? ''),
      categoryId: String(t.category?.id ?? ''),
      tourOperatorId: String(t.tourOperator?.id ?? ''),
      tagIds: (t.tags ?? []).map(x => x.id),
      imageFile: null
    });
  };

  const onDeleteTour = async (id) => {
    if (!window.confirm('Удалить экскурсию?')) return;
    await deleteTour(id);
    loadTours();
  };

  return (
    <div className="page">
      <div className="page-container">
        <h1 className="page-title">Панель администратора</h1>

        <div className="admin-grid">

          {/* ===== TOURS ===== */}
          <section className="admin-card">
            <h2>Экскурсии</h2>

            <input
              placeholder="Название"
              value={tourForm.title}
              onChange={e => handleTourChange('title', e.target.value)}
            />

            <textarea
              placeholder="Описание"
              value={tourForm.description}
              onChange={e => handleTourChange('description', e.target.value)}
            />

            <input
              type="number"
              placeholder="Цена"
              value={tourForm.price}
              onChange={e => handleTourChange('price', e.target.value)}
            />

            <input
              type="date"
              value={tourForm.startDate}
              onChange={e => handleTourChange('startDate', e.target.value)}
            />

            <input
              type="date"
              value={tourForm.endDate}
              onChange={e => handleTourChange('endDate', e.target.value)}
            />

            <select
              value={tourForm.cityId}
              onChange={e => handleTourChange('cityId', e.target.value)}
            >
              <option value="">Город</option>
              {cities.map(c => (
                <option key={c.id} value={c.id}>
                  {c.name} ({c.country?.name})
                </option>
              ))}
            </select>

            <select
              value={tourForm.categoryId}
              onChange={e => handleTourChange('categoryId', e.target.value)}
            >
              <option value="">Категория</option>
              {categories.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>

            <select
              value={tourForm.tourOperatorId}
              onChange={e => handleTourChange('tourOperatorId', e.target.value)}
            >
              <option value="">Туроператор</option>
              {operators.map(o => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>

            {/* TAGS */}
            <div className="admin-tags">
              {tags.map(tag => (
                <button
                  key={tag.id}
                  type="button"
                  className={`tag-btn ${tourForm.tagIds.includes(tag.id) ? 'active' : ''}`}
                  onClick={() => toggleTourTag(tag.id)}
                >
                  {tag.name}
                </button>
              ))}
            </div>

            {/* IMAGE */}
            <input
              type="file"
              accept="image/*"
              onChange={e => handleTourChange('imageFile', e.target.files?.[0] ?? null)}
            />

            <button className="admin-btn" onClick={onSaveTour}>
              {editingTourId ? 'Сохранить изменения' : 'Создать экскурсию'}
            </button>

            {editingTourId && (
              <button className="admin-btn" type="button" onClick={resetTourForm}>
                Отменить редактирование
              </button>
            )}

            <div className="admin-list">
              {tours.map(t => (
                <div key={t.id} className="admin-item">
                  <span>{t.title}</span>
                  <div className="admin-actions">
                    <img
                      src="/assets/svg/pen.svg"
                      alt="Редактировать"
                      onClick={() => onEditTour(t)}
                    />
                    <img
                      src="/assets/svg/bin.svg"
                      alt="Удалить"
                      onClick={() => onDeleteTour(t.id)}
                    />
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* ===== COUNTRIES ===== */}
          <section className="admin-card">
            <h2>Страны</h2>

            <input
              placeholder="Название страны"
              value={newCountry}
              onChange={e => setNewCountry(e.target.value)}
            />

            <button className="admin-btn" onClick={onAddCountry}>
              Добавить
            </button>

            <div className="admin-list">
              {countries.map(c => (
                <div key={c.id} className="admin-item">
                  <span>{c.name}</span>
                  <div className="admin-actions">
                    <img
                      src="/assets/svg/pen.svg"
                      alt="Редактировать"
                      onClick={() => onEditCountry(c)}
                    />
                    <img
                      src="/assets/svg/bin.svg"
                      alt="Удалить"
                      onClick={() => onDeleteCountry(c.id)}
                    />
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* ===== CITIES ===== */}
          <section className="admin-card">
            <h2>Города</h2>

            <input
              placeholder="Название города"
              value={newCity}
              onChange={e => setNewCity(e.target.value)}
            />

            <select
              value={cityCountryId}
              onChange={e => setCityCountryId(e.target.value)}
            >
              <option value="">Страна</option>
              {countries.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>

            <button className="admin-btn" onClick={onAddCity}>
              Добавить
            </button>

            <div className="admin-list">
              {cities.map(city => (
                <div key={city.id} className="admin-item">
                  <span>{city.name} ({city.country?.name})</span>
                  <div className="admin-actions">
                    <img
                      src="/assets/svg/pen.svg"
                      alt="Редактировать"
                      onClick={() => onEditCity(city)}
                    />
                    <img
                      src="/assets/svg/bin.svg"
                      alt="Удалить"
                      onClick={() => onDeleteCity(city.id)}
                    />
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* ===== CATEGORIES ===== */}
          <section className="admin-card">
            <h2>Категории</h2>

            <input
              placeholder="Название категории"
              value={newCategory}
              onChange={e => setNewCategory(e.target.value)}
            />

            <button className="admin-btn" onClick={onAddCategory}>
              Добавить
            </button>

            <div className="admin-list">
              {categories.map(cat => (
                <div key={cat.id} className="admin-item">
                  <span>{cat.name}</span>
                  <div className="admin-actions">
                    <img
                      src="/assets/svg/pen.svg"
                      alt="Редактировать"
                      onClick={() => onEditCategory(cat)}
                    />
                    <img
                      src="/assets/svg/bin.svg"
                      alt="Удалить"
                      onClick={() => onDeleteCategory(cat.id)}
                    />
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* ===== OPERATORS ===== */}
          <section className="admin-card">
            <h2>Туроператоры</h2>

            <input
              placeholder="Название туроператора"
              value={newOperator}
              onChange={e => setNewOperator(e.target.value)}
            />

            <button className="admin-btn" onClick={onAddOperator}>
              Добавить
            </button>

            <div className="admin-list">
              {operators.map(op => (
                <div key={op.id} className="admin-item">
                  <span>{op.name}</span>
                  <div className="admin-actions">
                    <img
                      src="/assets/svg/pen.svg"
                      alt="Редактировать"
                      onClick={() => onEditOperator(op)}
                    />
                    <img
                      src="/assets/svg/bin.svg"
                      alt="Удалить"
                      onClick={() => onDeleteOperator(op.id)}
                    />
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* ===== TAGS ===== */}
          <section className="admin-card">
            <h2>Теги</h2>

            <input
              placeholder="Название тега"
              value={newTag}
              onChange={e => setNewTag(e.target.value)}
            />

            <button className="admin-btn" onClick={onAddTag}>
              Добавить
            </button>

            <div className="admin-list">
              {tags.map(tag => (
                <div key={tag.id} className="admin-item">
                  <span>{tag.name}</span>
                  <div className="admin-actions">
                    <img
                      src="/assets/svg/pen.svg"
                      alt="Редактировать"
                      onClick={() => onEditTag(tag)}
                    />
                    <img
                      src="/assets/svg/bin.svg"
                      alt="Удалить"
                      onClick={() => onDeleteTag(tag.id)}
                    />
                  </div>
                </div>
              ))}
            </div>
          </section>

        </div>

        <button className="admin-report-btn" onClick={()=> setIsReportOpen(true)}>
          Скачать PDF-отчёт
        </button>
        {isReportOpen && (
        <ReportModal onClose={() => setIsReportOpen(false)} />
      )}

      </div>
    </div>
  );
};

export default AdminPanel;
