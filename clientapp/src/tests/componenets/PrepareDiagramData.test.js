import React, { PrepareDiagramData } from "../../components/Chart";

test("Rates should transfrom into correct from to make diagrams.", () => {
    const data = { labels: [], datasets: [] };
    const rates = [{
        "symbol": "BTC",
        "currentRate": 9168.16407336,
        "lastRefreshed": "2018-05-02 10:50:00",
        "timeZone": "UTC",
        "history": {
            "2018-05-02 10:50:00": 9168.16407336,
            "2018-05-02 10:45:00": 9181.72023583,
            "2018-05-02 10:40:00": 9191.14950357,
            "2018-05-02 10:35:00": 9176.50860017,
            "2018-05-02 10:30:00": 9176.60870585,
            "2018-05-02 10:25:00": 9186.47865752,
            "2018-05-02 10:20:00": 9197.13360156,
            "2018-05-02 10:15:00": 9185.86219835,
            "2018-05-02 10:10:00": 9169.16658602,
            "2018-05-02 10:05:00": 9152.87389199,
            "2018-05-02 10:00:00": 9147.15284856
        }
    }]
    PrepareDiagramData(rates,data,6)
    expect(data.datasets[0].data).toHaveLength(6);
})