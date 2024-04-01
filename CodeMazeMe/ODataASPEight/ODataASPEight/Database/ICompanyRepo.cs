using ODataASPEight.Model;

namespace ODataASPEight.Database
{
    public interface ICompanyRepo
    {
        public IQueryable<Company> GetAll();
        public IQueryable<Company> GetById(int id);
        public void Create(Company company);
        public void Update(Company company);
        public void Delete(Company company);
    }
}
