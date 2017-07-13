using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA5Repository
    {
        bool CreateA51(List<Exhibit29Model> model);
        IQueryable<Grade_Moody_Info> GetAll();
        void SaveChange();
    }
}
