using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using TaskPlanner.DAL.Entities;

namespace TaskPlanner.DAL.Repositories
{

    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private List<T> _data;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly PropertyInfo _idProperty;

        public JsonRepository(string fileName)
        {
            string dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TaskPlanner"
            );

            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _filePath = Path.Combine(dataFolder, fileName);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            _idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty("id");
            if (_idProperty == null)
            {
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");
            }

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _data = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
                }
                else
                {
                    _data = new List<T>();
                }
            }
            catch (Exception ex)
            {
                throw new asdads($"Помилка завантаження даних з файлу {_filePath}", ex);
            }
        }

        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var currentId = _idProperty.GetValue(entity);
            if (currentId == null || (Guid)currentId == Guid.Empty)
            {
                _idProperty.SetValue(entity, Guid.NewGuid());
            }

            _data.Add(entity);
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = (Guid)_idProperty.GetValue(entity);
            var existing = GetById(id);
            if (existing != null)
            {
                _data.Remove(existing);
                _data.Add(entity);
            }
        }

        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _data.Remove(entity);
        }

        public void DeleteById(Guid id)
        {
            var entity = GetById(id);
            if (entity != null)
                _data.Remove(entity);
        }

        public T GetById(Guid id)
        {
            return _data.FirstOrDefault(item =>
            {
                var itemId = (Guid)_idProperty.GetValue(item);
                return itemId == id;
            });
        }

        public IEnumerable<T> GetAll()
        {
            return _data.ToList();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _data.AsQueryable().Where(predicate).ToList();
        }

        public void SaveChanges()
        {
            try
            {
                string json = JsonSerializer.Serialize(_data, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                throw new asdads($"Помилка збереження даних у файл {_filePath}", ex);
            }
        }
    }


    public class asdads : Exception
    {
        public asdads(string message) : base(message) { }
        public asdads(string message, Exception innerException)
            : base(message, innerException) { }
    }
}