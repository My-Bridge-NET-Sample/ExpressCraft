﻿using static Retyped.dom;
using System;
using System.Collections.Generic;

namespace ExpressCraft
{
    public class DataRowEditForm : DialogForm
    {
        public bool LiveData;
        public GridView GridView;
        public DataRow DataRow;

        private HTMLDivElement Panel;

        private object[] prevData;

        public DataRowEditForm(DataRow _dataRow, GridView _gridView, bool _liveData = true) : base()
        {
            prevData = new object[_dataRow.ParentTable.ColumnCount];

            for(int i = 0; i < _dataRow.ParentTable.ColumnCount; i++)
            {
                prevData[i] = _dataRow[i];
            }

            DataRow = _dataRow;
            GridView = _gridView;
            LiveData = _liveData;

            this.Text = "Row Edit Form";
            this.Width = "400px"; // 25px - 25px 350px width;
            this.Height = "600px";
            this.Body.style.overflowY = "auto";

            Panel = Div();
            Panel.style.overflowY = "auto";
            Panel.SetBounds("0", "0", "100%", "(100% - 60px)");
            Body.style.backgroundColor = "white";

            _buttonCollection = new List<SimpleDialogButton>() {
                        new SimpleDialogButton(this, DialogResultEnum.Cancel) { Text = "Cancel", Location = new Vector2("(100% - 85px)", "(100% - 35px)"), ItemClick = (ev) => {
                            for (int i = 0; i < DataRow.ParentTable.ColumnCount; i++)
                            {
                                _dataRow[i] = prevData[i];
                            }

                            GridView.RenderGrid();
                        }},
                        new SimpleDialogButton(this, DialogResultEnum.OK) { Text = "OK", Location = new Vector2("(100% - 170px)", "(100% - 35px)")  }
                    };

            ButtonSection.AppendChildrenTabIndex(_buttonCollection.ToArray());

            this.Body.AppendChild(Panel);

            this.AllowSizeChange = false;
        }

        // Data now auto changes...
        protected override void OnClosed()
        {
            GridView.DataSource.EndDataUpdate();

            base.OnClosed();
        }

        protected override void OnShowed()
        {
            base.OnShowed();

            if(DataRow == null)
            {
                this.DialogResult = DialogResultEnum.Cancel;
                this.Close();
            }
            else
            {
                GridView.DataSource.BeginDataUpdate();

                GenerateForm();
            }
        }

        private void GenerateForm()
        {
            this.Panel.Empty();
            var length = GridView.ColumnCount();

            int col = 0;
            int height = 25;

            int defaultHeight = 24 + 3 + 24 + 3;
            int defaultHeight2X = defaultHeight * 3;
            int incrementHeight = defaultHeight;

            int eachWidth = (350 / 3) - 3;

            for(int i = 0; i < length; i++)
            {
                incrementHeight = defaultHeight;
                var grCol = GridView.GetColumn(i);

                if(!grCol.AllowEdit)
                    continue;

                var dtCol = grCol.Column;

                var dtIndex = grCol.GetDataColumnIndex();

                if(grCol.Column.FieldName.ToLower() == "cntr")
                {
                    grCol.ReadOnly = true;
                }

                switch(dtCol.DataType)
                {
                    case DataType.DateTime:
                        var lbldate = Label(grCol.Caption, 25 + (col * eachWidth + (col * 3)), height);
                        var inputDate = new TextInput("date");
                        inputDate.SetBounds(25 + (col * eachWidth + (col * 3)), height + 16 + 3, eachWidth, 24);
                        inputDate.SetDate(Convert.ToString(DataRow[dtIndex]));
                        inputDate.Readonly = grCol.ReadOnly;
                        if(!grCol.ReadOnly)
                        {
                            inputDate.OnTextChanged = (ev) =>
                            {
                                DataRow[dtIndex] = inputDate.GetDate();
                                if(LiveData)
                                    GridView.RenderGrid();
                            };
                        }

                        Panel.AppendChildren(lbldate, inputDate);

                        break;

                    case DataType.Integer:
                    case DataType.Long:
                    case DataType.Float:
                    case DataType.Double:
                    case DataType.Decimal:
                    case DataType.Bool:
                    case DataType.Byte:
                    case DataType.Short:

                        var lblnmb = Label(grCol.Caption, 25 + (col * eachWidth + (col * 3)), height);
                        TextInput inputNum;
                        if(grCol.CellDisplay is GridViewCellDisplayCheckBox)
                        {
                            inputNum = new TextInput("checkbox");
                            inputNum.SetChecked(DataRow[dtIndex]);
                        }
                        else
                        {
                            inputNum = new TextInput("number");
                            inputNum.Text = Convert.ToString(DataRow[dtIndex]);
                        }

                        inputNum.SetBounds(25 + (col * eachWidth + (col * 3)), height + 16 + 3, eachWidth, 24);

                        inputNum.Readonly = grCol.ReadOnly;

                        if(!grCol.ReadOnly)
                        {
                            inputNum.OnTextChanged = (ev) =>
                            {
                                if(inputNum.Type == "checkbox")
                                {
                                    DataRow[dtIndex] = inputNum.Text.IsTrue() == 1;
                                }
                                else
                                {
                                    DataRow[dtIndex] = inputNum.Text;
                                }
                                if(LiveData)
                                    GridView.RenderGrid();
                            };
                        }

                        Panel.AppendChildren(lblnmb, inputNum);
                        break;

                    default:
                    case DataType.Object:
                    case DataType.String:
                        var lblstr = Label(grCol.Caption, 25 + (col * eachWidth + (col * 3)), height);
                        var inputstr = new TextInput("text");
                        inputstr.SetBounds(25 + (col * eachWidth + (col * 3)), height + 16 + 3, eachWidth, 24);
                        inputstr.Text = Convert.ToString(DataRow[dtIndex]);
                        inputstr.Readonly = grCol.ReadOnly;

                        if(!grCol.ReadOnly)
                        {
                            inputstr.OnTextChanged = (ev) =>
                            {
                                DataRow[dtIndex] = inputstr.Text;

                                if(LiveData)
                                    GridView.RenderGrid();
                            };
                        }

                        Panel.AppendChildren(lblstr, inputstr);

                        //if(obj.Length > 100)
                        //{
                        //	incrementHeight = defaultHeight2X;

                        //	col = 2;
                        //}
                        //else
                        //{
                        //}

                        break;
                }
                if(col == 2)
                {
                    height += incrementHeight + 3;
                    col = 0;
                }
                else
                {
                    col++;
                }
            }
            // Add Accept Changes
        }
    }
}