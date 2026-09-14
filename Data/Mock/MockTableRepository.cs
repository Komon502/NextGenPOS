using System.Collections.Generic;
using System.Linq;
using NextGenPOS.Data;
using NextGenPOS.Models;

namespace NextGenPOS.Data.Mock
{
    public class MockTableRepository : ITableRepository
    {
        private readonly List<Table> _tables;
        private int _nextId;

        public MockTableRepository()
        {
            _tables = new List<Table>
            {
                new Table { TableId=1, TableName="T1", Seats=2, Status="Available", ZoneArea="Floor" },
                new Table { TableId=2, TableName="T2", Seats=4, Status="Available", ZoneArea="Floor" },
                new Table { TableId=3, TableName="T3", Seats=4, Status="Occupied",  ZoneArea="Floor" },
                new Table { TableId=4, TableName="T4", Seats=6, Status="Available", ZoneArea="Floor" },
                new Table { TableId=5, TableName="T5", Seats=2, Status="Reserved",  ZoneArea="Floor" },
                new Table { TableId=6, TableName="T6", Seats=8, Status="Available", ZoneArea="Floor" },
                new Table { TableId=7, TableName="P1", Seats=4, Status="Available", ZoneArea="Patio" },
                new Table { TableId=8, TableName="P2", Seats=4, Status="Occupied",  ZoneArea="Patio" },
                new Table { TableId=9, TableName="B1", Seats=2, Status="Available", ZoneArea="Bar" },
                new Table { TableId=10,TableName="B2", Seats=2, Status="Available", ZoneArea="Bar" },
            };
            _nextId = 11;
        }

        public List<Table> GetAll() => _tables.ToList();
        public Table GetById(int id) => _tables.FirstOrDefault(t => t.TableId == id);
        public int Insert(Table t) { t.TableId = _nextId++; _tables.Add(t); return t.TableId; }
        public void Update(Table t) { var e = GetById(t.TableId); if (e != null) { e.TableName=t.TableName; e.Seats=t.Seats; e.Status=t.Status; e.ZoneArea=t.ZoneArea; } }
        public void Delete(int id) { _tables.RemoveAll(t => t.TableId == id); }
        public void UpdateStatus(int tableId, string status) { var t = GetById(tableId); if (t != null) t.Status = status; }
    }
}
