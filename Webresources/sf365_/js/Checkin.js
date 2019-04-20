/// <reference path="../../node_modules/@types/knockout/index.d.ts" />
var sf365;
(function (sf365) {
    var Checkin;
    (function (Checkin) {
        var CheckInViewModel = /** @class */ (function () {
            function CheckInViewModel() {
                this.isbusy = ko.observable(false);
            }
            CheckInViewModel.prototype.foo = function () {
                // Some change
                alert("bar");
            };
            return CheckInViewModel;
        }());
    })(Checkin = sf365.Checkin || (sf365.Checkin = {}));
})(sf365 || (sf365 = {}));
//# sourceMappingURL=Checkin.js.map