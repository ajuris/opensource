(function ($) {
    var controllerUrl = '/modules/geta.cachemanager/admin/cachemanagercontroller.ashx';
    var cacheManager = {
        table: null,
        cacheListTable: null,
        init: function () {
            cacheManager.table = $('[data-role="server-stats-table"]');
            cacheManager.cacheListTable = $('[data-role="cache-list-table"]');

            $('[data-role="clear-cache-button"]').on('click', function () {
                cacheManager.clearCache();
            });
            $('[data-role="refresh-stats-button"]').on('click', function () {
                cacheManager.refresh();
            });
            $('#cm-checkbox-select-all').on('click', function () {
                cacheManager.selectAllCacheItem($(this).is(':checked'));
            });
            $('[data-role="delete-selected-cache-button"]').on('click', function () {
                cacheManager.deleteSelectedCacheItem();
            });

            cacheManager.refresh();
        },

        refresh: function () {
            cacheManager.getInfo();
            cacheManager.getCacheList();
        },

        getInfo: function () {
            if (!cacheManager.table) {
                return;
            }
            $.getJSON(controllerUrl, { cmd: 'getinfo' }, function (data) {
                if (data != null) {
                    var serverName = data.MachineName,
                        availableMemory = data.AvailableMemory + ' MB',
                        applicationPath = data.ApplicationPath,
                        appCacheEntries = data.AppCacheEntries;

                    cacheManager.table.find('tbody')
                        .html('<tr><td>Server Name</td><td>' + serverName + '</td></tr><tr><td>Free Memory</td><td>' 
                            + availableMemory + '</td></tr><tr><td>Application Path</td><td>' + applicationPath 
                            + '</td></tr><tr><td>Application Cache Entries</td><td>' + appCacheEntries + '</td></tr>');
                }
            });
        },

        clearCache: function () {
            $.getJSON(controllerUrl, { cmd: 'clearcache' }, function (data) {
                if (data != null) {
                    if (data) {
                        cacheManager.refresh();
                    }
                }
            });
        },

        getCacheList: function () {
            if (!cacheManager.cacheListTable) {
                return;
            }

            var tbody = cacheManager.cacheListTable.find('tbody');
            var rows = '';
            $.getJSON(controllerUrl, { cmd: 'getcachelist' }, function (data) {
                if (data != null) {
                    for (var i in data) {
                        rows += '<tr><td><input name="cacheItem" type="checkbox" value="' + data[i] + '"></td><td>' + data[i] + '</td></tr>';
                    }
                    tbody.html(rows);
                }
            }).error(function (jqXHR, textStatus, errorThrown) {
                tbody.html('<tr><td colspan="2"><p class=EP-systemMessage EP-systemMessage-None">' + errorThrown + '</p></td></tr>');
            });
        },

        selectAllCacheItem: function (select) {
            if (!cacheManager.cacheListTable) {
                return;
            }

            cacheManager.cacheListTable.find('input[type="checkbox"]').each(function () {
                this.checked = select;
            });
        },

        deleteSelectedCacheItem: function () {
            if (!cacheManager.cacheListTable) {
                return;
            }

            var checkboxes = cacheManager.cacheListTable.find('tbody:first').find('input[type="checkbox"]:checked');

            if (checkboxes.length) {
                $.ajax({
                    url: controllerUrl,
                    type: 'POST',
                    dataType: 'json',
                    data: {
                        cmd: 'deleteselectedcache',
                        cachelist: JSON.stringify($(checkboxes).serializeArray(), null, 2)
                    },
                    success: function(data) {
                        if (data != null) {
                            if (data) {
                                cacheManager.cacheListTable.find('input[type="checkbox"]').each(function() { this.checked = false; });
                                cacheManager.refresh();
                            }
                        }
                    }
                });
            }
        }
    };

    $(function () {
        cacheManager.init();
    });
})(jQuery);