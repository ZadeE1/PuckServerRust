//! Tick timing + network counters (single-threaded game loop owns them).

#[derive(Debug, Default)]
pub struct TickStats {
    pub count: u64,
    pub total_micros: u128,
    pub max_micros: u128,
    pub last_micros: u128,
    pub inputs: u64,
    pub snapshots: u64,
    pub events_sent: u64,
    pub events_acked: u64,
    pub joins: u64,
    pub rejects: u64,
}

impl TickStats {
    pub fn record(&mut self, micros: u128) {
        self.count += 1;
        self.total_micros += micros;
        self.max_micros = self.max_micros.max(micros);
        self.last_micros = micros;
    }

    pub fn avg_micros(&self) -> f64 {
        if self.count == 0 {
            0.0
        } else {
            self.total_micros as f64 / self.count as f64
        }
    }
}
