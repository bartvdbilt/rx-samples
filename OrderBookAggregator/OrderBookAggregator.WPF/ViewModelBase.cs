using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrderBookAggregator.WPF
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">property</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            var body = propertyExpression.Body as MemberExpression;
            if (body != null)
            {
                var property = body.Member as PropertyInfo;
                if (property != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property.Name));
                }
            }

        }
    }
}
