using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transfer.Models.Interface;
using Transfer.ViewModels;

namespace Transfer.Models.Repository
{
    public class A5Repository : IA5Repository , IDbEvent
    {
        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public A5Repository()
        {
            this.db = new IFRS9Entities();
        }

        public bool CreateA51(List<Exhibit29Model> model)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Grade_Moody_Info> GetAll()
        {
            throw new NotImplementedException();
        }

        public void SaveChange()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.db != null)
                {
                    this.db.Dispose();
                    this.db = null;
                }
            }
        }
    }
}