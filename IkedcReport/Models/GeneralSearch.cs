using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class GeneralSearch
    {
        public String SearchCriteria { get; set; }
        public String SearchValue { get; set; }
        private int _CurrentPageIndex;
        public int CurrentPageIndex
        {
            get
            {
                if(_CurrentPageIndex == 0)
                {
                    return 1;
                }
                return _CurrentPageIndex;
            }
            set
            {
                _CurrentPageIndex = value;
            }
        }
        private int _PageSize;
        public int PageSize
        {
            get
            {
                if (_PageSize == 0)
                {
                    _PageSize = 15;
                }
                return _PageSize;
            }
            set
            {
                if (value < 15)
                {
                    _PageSize = 15;
                }
                else
                {
                    _PageSize = value;
                }
            }
        }
        public int ItemCount { get; set; }
        public int Skip { get; set; }
    }
}