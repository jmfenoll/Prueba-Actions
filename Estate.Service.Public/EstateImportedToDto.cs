using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using WayToCol.Common.Api.Helpers;
using WayToCol.Common.Contracts.Estates;

namespace WayToCol.Estate.Service.Public
{
    class EstateImportedToDto
    {
        private propiedadesPropiedadXml _from;

        //private Dictionary<string, Func<object, propiedadesPropdiedadXml, object>> _propertyCast = new Dictionary<string, Func<object, propiedadesPropiedadXml, object>>
        //{
        //    // Aquí declararemos las excepciones al mapeo,
        //    // el resto se mapearán por coincidencia de nombre, y con el tipo declarado
        //    //
        //    {nameof(EstateDto.id), (value, obj) => { return ModelHelper.GetMD5(obj.id);} },
        //    {nameof(EstateDto.id_ficha), (value, obj) => { return obj.id; } },
        //    {nameof(EstateDto.numpanos), (value, obj) => { return null; } },
        //};

        List<(string propertyFrom, string propertyTo, Func<object, propiedadesPropiedadXml, object> converter)> _propertyMap
            = new List<(string propertyFrom, string propertyTo, Func<object, propiedadesPropiedadXml, object> converter)>
        {
                (nameof(propiedadesPropiedadXml.id), nameof(EstateDto.fichaId), ToInt),
                (nameof(propiedadesPropiedadXml.numpanos), null, null),
                (nameof(propiedadesPropiedadXml.altitud), nameof(EstateDto.longitud), ToDec),
                (null, nameof(EstateDto.id),(value, obj)=>{ return ModelHelper.GetMD5(obj.id); }),
        };




        public EstateImportedToDto(propiedadesPropiedadXml from)
        {
            _from = from;
        }

        public EstateDto ConvertToDto()
        {

            if (_from == null) return null;

            var to = new EstateDto();

            // Recorre todas las propiedades de from 
            foreach (var property in _from.GetType().GetProperties())
            {
                try
                {
                    object value = property.GetValue(_from);
                    var toProperty = to.GetType().GetProperty(property.Name);
                    // si no hay un macheo de nombre, busca en el mapping si hay excepción
                    if (toProperty == null)
                    {
                        var propName = _propertyMap.Where(x => x.propertyFrom == property.Name).FirstOrDefault().propertyTo;
                        if (propName == null)
                            continue;
                        toProperty = to.GetType().GetProperty(propName);
                    }

                    // Si está en el mapeo
                    var mapping = _propertyMap.Where(x => x.propertyFrom == property.Name).FirstOrDefault();
                    if (!(mapping.converter == null && mapping.propertyFrom == null && mapping.propertyTo == null))
                    {
                        // Si el conversor es nulo, pasamos al siguiente
                        if (mapping.converter == null)
                        {
                            _propertyMap.Remove(mapping);
                            continue;
                        }
                        object valueCasted = mapping.converter(value, _from);
                        if (mapping.propertyTo != null)
                            toProperty = to.GetType().GetProperty(mapping.propertyTo);

                        toProperty.SetValue(to, valueCasted, null);
                        _propertyMap.Remove(mapping);
                    }
                    else if (toProperty.PropertyType == typeof(string))
                    {
                        toProperty.SetValue(to, value, null);
                    }
                    else if (new[] { typeof(decimal), typeof(decimal?) }.Contains(toProperty.PropertyType))
                    {
                        if (!IsNull(value)) toProperty.SetValue(to, ToDec(value), null);
                    }
                    else if (new[] { typeof(int), typeof(int?) }.Contains(toProperty.PropertyType))
                    {
                        if (!IsNull(value)) toProperty.SetValue(to, ToInt(value), null);
                    }
                    else if (new[] { typeof(bool), typeof(bool?) }.Contains(toProperty.PropertyType))
                    {
                        if (!IsNull(value)) toProperty.SetValue(to, ToBool(value), null);
                    }
                    else if (new[] { typeof(DateTime), typeof(DateTime?) }.Contains(toProperty.PropertyType))
                    {
                        if (!IsNull(value)) toProperty.SetValue(to, ToDateTime(value), null);
                    }
                    else
                        throw new ApplicationException($"tipo {toProperty.PropertyType} no controlado en propiedad {property.Name}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Cast exception trying to cast {property.Name}: {ex.Message}");
                }
            }

            // Ahora tratamos las propiedades que no están en origen y están en destino
            foreach (var mapping in _propertyMap.Where(x => x.propertyFrom == null && x.converter != null))
            {
                var toProperty = to.GetType().GetProperty(mapping.propertyTo);
                object valueCasted = mapping.converter(null, _from);
                toProperty.SetValue(to, valueCasted, null);
            }
            return to;
        }

        private bool IsNull(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true;
            return false;
        }

        private static object ToBool(object value, propiedadesPropiedadXml obj = null)
        {
            return (value as string) == "1" ? true : false;
        }

        private static object ToInt(object value, propiedadesPropiedadXml obj = null)
        {
            return int.Parse(value as string);
        }

        private static object ToDec(object value, propiedadesPropiedadXml obj = null)
        {
            return decimal.Parse(value as string, CultureInfo.InvariantCulture);
        }

        private static object ToStr(object value, propiedadesPropiedadXml obj = null)
        {
            return value as string;
        }

        private static object ToDateTime(object value, propiedadesPropiedadXml obj = null)
        {
            return DateTime.Parse(value as string);
        }
    }
}
