/**
 * @version 1.0.0.0
 * @copyright Copyright ©  2017
 * @compiler Bridge.NET 15.6.0
 */
Bridge.assembly("ExpressCraftGridView", function ($asm, globals) {
    "use strict";

    Bridge.define("ExpressCraftGridView.App", {
        $main: function () {
            ExpressCraft.Form.setup();

            ExpressCraft.Application.run(new ExpressCraftGridView.App.GridForm());
        }
    });

    Bridge.define("ExpressCraftGridView.App.GridForm", {
        inherits: [ExpressCraft.Form],
        gridView: null,
        addNewRowButton: null,
        add100000RowsButton: null,
        clearRowsButton: null,
        x: 0,
        ctor: function () {
            this.$initialize();
            ExpressCraft.Form.ctor.call(this);
            this.gridView = new ExpressCraft.GridView(true, true);

            var dataTable = new ExpressCraft.DataTable();

            dataTable.addColumn("Account Number", ExpressCraft.DataType.Integer);
            dataTable.addColumn("Last Name", ExpressCraft.DataType.String);
            dataTable.addColumn("First Name", ExpressCraft.DataType.String);
            dataTable.addColumn("Date Contacted", ExpressCraft.DataType.DateTime);

            this.gridView.setDataSource(dataTable);

            ExpressCraft.Helper.setBoundsFull(this.gridView);

            this.addNewRowButton = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: "Add New a Row"
            } );
            ExpressCraft.Helper.setBounds$5(this.addNewRowButton, "3px", "3px", "auto", "24px");

            this.add100000RowsButton = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: "Add 100000 Row's"
            } );
            ExpressCraft.Helper.setBounds$5(this.add100000RowsButton, "98px", "3px", "auto", "24px");

            this.clearRowsButton = Bridge.merge(new ExpressCraft.SimpleButton(), {
                setText: "Clear Rows"
            } );
            ExpressCraft.Helper.setBounds$5(this.clearRowsButton, "205px", "3px", "auto", "24px");

            this.clearRowsButton.itemClick = Bridge.fn.bind(this, function (ev) {
                dataTable.clearRows();

                this.gridView.renderGrid();
            });

            this.add100000RowsButton.itemClick = Bridge.fn.bind(this, function (ev) {
                dataTable.beginNewRow(100000);

                for (var i = 0; i < 100000; i = (i + 1) | 0) {
                    this.x = (this.x + 1) | 0;
                    var dr = dataTable.newRow();
                    dr.setItem(0, this.x);
                    dr.setItem(1, "Some Last Name");
                    dr.setItem(2, "Some First Name");
                    dr.setItem(3, Bridge.Date.today());
                }

                dataTable.acceptNewRows();

                this.gridView.renderGrid();
                this.gridView.scrollToBottom();
            });

            this.addNewRowButton.itemClick = Bridge.fn.bind(this, function (ev) {
                var dr = dataTable.newRow();
                var fdre = new ExpressCraft.FormDataRowEdit(dr, this.gridView, true);

                fdre.showDialog([new ExpressCraft.DialogResult(ExpressCraft.DialogResultEnum.OK, Bridge.fn.bind(this, function () {
                    dataTable.acceptNewRows();
                    this.gridView.renderGrid();
                })), new ExpressCraft.DialogResult(ExpressCraft.DialogResultEnum.Cancel, function () {
                    dataTable.rejectNewRows();
                })]);
            });

            ExpressCraft.Helper.appendChildren$1(this.getHeading(), [this.addNewRowButton, this.add100000RowsButton, this.clearRowsButton]);
            this.getBody().appendChild(ExpressCraft.Control.op_Implicit(this.gridView));

            this.linkchildToForm(this.gridView);

            this.gridView.renderGrid();
        },
        onShowing: function () {
            ExpressCraft.Form.prototype.onShowing.call(this);
        }
    });
});
