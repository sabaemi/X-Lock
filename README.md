Locking is one of the methods of preventing the problems of producing incorrect results by executing transactions in the database. Unfortunately, locking can lead to a deadlock, and every deadlock must be detected and resolved. One way to resolve any deadlock is to break the waiting loop, which fortunately has the possibility of rollback in the database, which has its disadvantages far less than killing the transaction. 
In this program, it is assumed that two transactions are executed by two threads at the same time and use a common database. For simplicity, the following assumptions are made:
1. The database consists of only one table that has only 100 records.
2. There is only one type of lock in the system, which is X-lock with the same standard protocol of this lock. Therefore, the same exclusive lock must be used even for reading records. These threads accidentally (using the ransom number generator) lock the record number they need and do it so often that a deadlock occurs. The task of the deadlock detection routine is to detect the deadlock and Fix it. Once we have correctly identified the deadlock and are sure that the program is working properly, we just exit the program.

Notes:
1. We have two parallel threads
2. To determine which records are locked by which transaction, we mention the name of the transaction in front of each record.
3. We use the database (here) and communicate with it.
4. The deadlock detection part is allowed to read any record in any order and at any time without needing to lock it.
