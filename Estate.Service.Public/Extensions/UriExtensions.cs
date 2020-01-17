using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WayToCol.Estate.Service.Public.Extensions
{
    public static class UriExtensions
    {
        public static Uri Append(this Uri uri, params string[] paths)
        {
            var url = uri.AbsoluteUri.TrimEnd('/');

            foreach (var p in paths)
            {
                url += $"/{p.TrimStart('/').TrimEnd('/')}";
            }
            return new Uri(url);
        }
    }
}
