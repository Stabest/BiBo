﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BiBo.Persons;

using BiBo.DAO;

namespace BiBo
{
    class ExemplarDAO : SqlConnector<Exemplar>
    {
        //Member-Variablen Deklaration
        private DateTime loanPeriod;     //Ausleifrist 
        private BookStates state;		    //Status des Buches (only_visible, damaged, missing)
        private string signatur; 	    //signatur des buches
        private Access accesser; 	    //Zugang zum Exemplar (magazin, freihandausleihe)
        private Customer borrower;	    //Ausleiher
        private ulong exemplarId;     //Exemplar-Nummer
        private ulong bookId;         //dazugehörige Buch-ID
    
        public ExemplarDAO()
        {
            string exemplarSQL = @"CREATE TABLE IF NOT EXISTS Exemplar (
                                       ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,  
                                       loanPeriod DateTime, 
                                       state VARCHAR(100) NOT NULL,
                                       signatur VARCHAR(100) NOT NULL, 
                                       access VARCHAR(100) NOT NULL,
                                       customerID INTEGER,
                                       bookID INTEGER,
                                    );";

            SQLiteCommand command = new SQLiteCommand(exemplarSQL, con);
            command.ExecuteNonQuery();
        }

        public override bool AddEntry(Exemplar exemplar)
        {
            SQLiteCommand command = new SQLiteCommand(con);
            command.CommandText = @"INSERT INTO Book (
                                      id, 
                                      state, 
                                      signatur, 
                                      access
                                  )   
                                  VALUES (
                                      NULL,  
                                      '" + exemplar.State + @"',
                                      '" + exemplar.Signatur + @"',
                                      '" + exemplar.Accesser.ToString() + @"'
                                  );";

            command.ExecuteNonQuery();
            return true;
        }

        public override ulong AddEntryReturnId(Exemplar exemplar)
        {
            AddEntry(exemplar);

            SQLiteCommand command = new SQLiteCommand(con);
            command.CommandText = "select last_insert_rowid()";
            UInt64 lastRowID64 = (UInt64)command.ExecuteScalar();
            return (ulong)lastRowID64;
        }

        public override bool DeleteEntry(Exemplar exemplar)
        {
            SQLiteCommand command = new SQLiteCommand(con);
            command.CommandText = "DELETE FROM Exemplar WHERE author='" + exemplar.ExemplarId + "';";
            command.ExecuteNonQuery();
            return true;
        }

        public override bool DeleteEntryByIdList(List<ulong> l)
        {
            foreach (ulong x in l)
            {
                SQLiteCommand command = new SQLiteCommand(con);
                command.CommandText = "DELETE FROM Customer WHERE id ='" + x + "';";
                command.ExecuteNonQuery();

            }
            return true;
        }

        public override Exemplar GetEntryById(ulong id)
        {
            SQLiteCommand command = new SQLiteCommand(con);
            command.CommandText = "SELECT * FROM Exemplar WHERE id = '" + id + "'";
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                return InitEntryByReader(reader);
            }

            else
                throw new Exception("Eintrag nicht vorhanden");
        }

        public override List<Exemplar> GetAllEntrys()
        {
            List<Exemplar> exemplarList = new List<Exemplar>();

            SQLiteCommand command = new SQLiteCommand(con);
            command.CommandText = "SELECT * FROM Exemplar";
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    exemplarList.Add(InitEntryByReader(reader));
                }
            }

            return exemplarList;
        }

        protected override Exemplar InitEntryByReader(System.Data.SQLite.SQLiteDataReader reader)
        {
            Exemplar exemplar = new Exemplar();
            
            ulong id = System.Convert.ToUInt64(reader.GetInt32(reader.GetOrdinal("id")));

            string loanPeriodAsString = reader.GetString(reader.GetOrdinal("loanPeriod"));
            DateTime loanPeriod = new DateTime();
            if(loanPeriodAsString != null || loanPeriodAsString != "")
                loanPeriod = DateTime.Parse(loanPeriodAsString);

            string stateString = reader.GetString(reader.GetOrdinal("state"));
            BookStates state = (BookStates) Enum.Parse(typeof(BookStates), stateString, true);

            string signatur = reader.GetString(reader.GetOrdinal("signatur"));
            ulong customerId = System.Convert.ToUInt64(reader.GetInt32(reader.GetOrdinal("customerID")));
            ulong bookId = System.Convert.ToUInt64(reader.GetInt32(reader.GetOrdinal("bookID")));
            
            exemplar.ExemplarId = id;
            exemplar.LoanPeriod = loanPeriod;
            exemplar.State = state;
            exemplar.Signatur = signatur;
            exemplar.BookId = bookId;

            return exemplar;
        }
    }   
}