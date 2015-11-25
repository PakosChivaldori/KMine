using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kmine
{
    /// <summary>
    /// �������� ������
    /// </summary>
    public class GridCell
    {
        /// <summary>
        /// � ������ ����
        /// </summary>
        public bool Flag { get { return Number == 9; } set { if (value) Number = 9; } }
        /// <summary>
        /// ���������� �.�����
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// ��� �� �������
        /// </summary>
        public bool Free { get { return Number == 0; } }
        /// <summary>
        /// �����
        /// </summary>
        public bool Empty { get { return Number == -1; } set { if (value) Number = -1; } }
        public GridCell()
        {
            Number = 0;
        }
    }
}