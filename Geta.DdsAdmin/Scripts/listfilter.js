(function ($) {
    var methods = {
        init: function (options) {
            var settings = $.extend({
                listName: null
            }, options);
            
            return this.each(function () {
                var $this = $(this);
                var listItems = (settings.listName == null) ? $this.closest('.div').find('li') : $(settings.listName).find('li');
                var textboxFilterValue = '';

                $this.bind('keyup paste', function () {
                    textboxFilterValue = $(this).val();
                    methods.filterListItem(listItems, textboxFilterValue);
                });
            });
        },
        filterListItem: function (listItems, textboxFilterValue) {
            listItems.each(function () {
                var li = $(this);
                (methods.filterByTextBox(li, textboxFilterValue)) ? li.show() : li.hide();
            });
        },
        filterByTextBox: function (li, filterValue) {
            var filterValues = filterValue.toLowerCase().split(' ');

            for (i in filterValues) {
                if (li.text().toLowerCase().indexOf(filterValues[i]) === -1) {
                    return false;
                }
            }
            return true;
        }
    };

    $.fn.listFilter = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.listFilter');
        }
    };
})(jQuery);