using System;
using System.Collections;
using System.Collections.Generic;

public class QuestLog : IEnumerable<Quest.LogEntry>
{
    private LinkedList<Quest.LogEntry> log;

    public QuestLog()
    {
        log = new LinkedList<Quest.LogEntry>();
    }
    
    public void Add(Quest.LogEntry logEntry)
    {
        log.AddLast(logEntry);
    }
    
    public IEnumerator<Quest.LogEntry> GetEnumerator()
    {
        return log.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
