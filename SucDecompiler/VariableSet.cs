using banallib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SucDecompiler
{
    public class VariableSet
    {
        public Dictionary<short, Variable> Variables { get; set; }
        public List<ValueViewModel> ValueList { get; set; }

        public void BuildVariables(ParsedContent parsedContent)
        {
            Variables = new Dictionary<short, Variable>();
            ValueList = new List<ValueViewModel>();

            this.ValueList = new List<ValueViewModel>();
            foreach (Value value in parsedContent.ValuesList)
            {
                ValueViewModel valueViewModel = new ValueViewModel()
                {
                    Address = value.Address,
                    AddressHex = (short)(value.Address - parsedContent.BaseAddress + parsedContent.StartOfValues),
                    AddressHexBase = (short)(value.Address + parsedContent.StartOfValues),
                    DataType = value.DataType,
                    Reference = value.Reference,
                    IsMe = value.IsMe,
                    IsPlayer = value.IsPlayer
                };
                if (value.SubValues.Count > 0)
                {
                    if (valueViewModel.DataType == DataTypeType.Float)
                    {
                        valueViewModel.SubValue1 = BitConverter.ToSingle(BitConverter.GetBytes((Int32)(value.SubValues[0])), 0);
                    }
                    else
                    {
                        valueViewModel.SubValue1 = value.SubValues[0];
                    }
                }
                if (value.SubValues.Count > 1)
                {
                    valueViewModel.SubValue2 = value.SubValues[1];
                }
                if (value.SubValues.Count > 2)
                {
                    valueViewModel.SubValue3 = value.SubValues[2];
                }
                if (value.SubValues.Count > 3)
                {
                    valueViewModel.SubValue4 = value.SubValues[3];
                }
                if (value.SubValues.Count > 4)
                {
                    valueViewModel.SubValue5 = value.SubValues[4];
                }

                this.ValueList.Add(valueViewModel);


                //build variables list
                if (!Variables.ContainsKey(value.Address))
                {
                    Variable v = new Variable()
                    {
                        Address = value.Address,
                        DataType = value.DataType.ToString(),
                        //Name = "var" + value.DataType.ToString() + value.Address.ToString()
                    };

                    if (value.IsMe
                        || value.IsPlayer)
                    {
                        v.Used = true;
                        v.Static = true;
                    }

                    switch (v.DataType.ToLower())
                    {
                        case ("int"):
                            v.Name = "nVar" + v.Address.ToString();
                            break;
                        case ("string"):
                            v.Name = "szVar" + v.Address.ToString();
                            break;
                        case ("point"):
                            v.Name = "ptVar" + v.Address.ToString();
                            v.Static = false; //points are never static
                            break;
                        case ("character"):
                            v.Name = "cVar" + v.Address.ToString();
                            break;
                        case ("float"):
                            v.Name = "fVar" + v.Address.ToString();
                            break;
                        case ("quaternion"):
                            v.Name = "qVar" + v.Address.ToString();
                            break;
                    }
                    Variables.Add(value.Address, v);
                }
            }

            //work out which variables are static or unused
            for (int i = 0; i < parsedContent.OpCodeList.Count; i++)
            {
                Operation operation = parsedContent.OpCodeList[i];
                if (operation.OpCode == OpCodeType.OP_GETTOP)
                {
                    if (Variables.ContainsKey(operation.DataIndex.Value))
                    {
                        Variables[operation.DataIndex.Value].Static = false;
                        Variables[operation.DataIndex.Value].Used = true;
                    }
                }
                else if (operation.OpCode == OpCodeType.OP_PUSH)
                {
                    if (Variables.ContainsKey(operation.DataIndex.Value))
                    {
                        Variables[operation.DataIndex.Value].Used = true;
                    }
                }
            }
        }

        public object GetCurrentValue(short address)
        {
            var q = from v in ValueList
                    where v.Address == address
                    select v;
            ValueViewModel vvm = q.FirstOrDefault();

            StringBuilder sb = new StringBuilder();

            if (vvm.IsMe)
            {
                sb.Append("Me");
                return sb.ToString();
            }
            else if (vvm.IsPlayer)
            {
                sb.Append("Player");
                return sb.ToString();
            }

            if (vvm.SubValue1 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue1));
            }
            if (vvm.SubValue2 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue2));
            }
            if (vvm.SubValue3 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue3));
            }
            if (vvm.SubValue4 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue4));
            }
            if (vvm.SubValue5 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue5));
            }

            return sb.ToString();
        }

        public string GetSubValue(ValueViewModel vvm, object sv)
        {
            //if (vvm.DataType == DataTypeType.String)
            //{
            //    return string.Format("\"{0}\"", sv);
            //}

            return sv.ToString();
        }
    }
}
