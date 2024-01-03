
require.config({
    baseUrl: '/subtitle-editor',
    paths: {
        '@microsoft/signalr': `lib/signalr/signalr`
    }
})

const modules = ['@microsoft/signalr',
    'uform-utility', 'uform-dropdown', 'uform-popover', 'uform-api', 'uform-dialog', 'uform-selector', 'uform-datepicker', 'uform-form',
    'uform-form-selector', 'uform-form-datepicker', 'uform-form-time',
    'SubtitleEditor', 'App'
];
require(modules, function () { });