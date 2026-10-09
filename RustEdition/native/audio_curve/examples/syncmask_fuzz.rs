// Fuzz oracle for sync_mask: reads case lines from a file, prints one mask per line.
// Line format: 13 shorts,comp_u32,div | 13 shorts,comp_u32,div | high
// Example: 0,0,0,0,0,0,0,0,0,0,0,0,0,0,1|3,0,0,0,0,0,0,0,0,0,0,0,0,0,1|0
// Not shipped in the cdylib; Temp-harness use only.
use audio_curve::{SyncMaskInput, sync_mask};
use std::io::{BufRead, BufReader};

fn parse_side(s: &str) -> SyncMaskInput {
    let p: Vec<&str> = s.split(',').collect();
    assert_eq!(p.len(), 15, "side needs 13 shorts + comp + div");
    let g = |i: usize| p[i].parse::<i16>().unwrap();
    SyncMaskInput {
        x: g(0),
        y: g(1),
        z: g(2),
        rx: g(3),
        ry: g(4),
        rz: g(5),
        rw: g(6),
        vx: g(7),
        vy: g(8),
        vz: g(9),
        ax: g(10),
        ay: g(11),
        az: g(12),
        tick_rate_divisor: p[14].parse::<u8>().unwrap(),
        compressed_rotation: p[13].parse::<u32>().unwrap(),
    }
}

fn main() {
    let path = std::env::args().nth(1).expect("usage: syncmask_fuzz <cases.txt>");
    let f = BufReader::new(std::fs::File::open(path).unwrap());
    for line in f.lines() {
        let line = line.unwrap();
        if line.trim().is_empty() {
            continue;
        }
        let parts: Vec<&str> = line.split('|').collect();
        let mask = sync_mask(parse_side(parts[0]), parse_side(parts[1]), parts[2].parse::<i32>().unwrap());
        println!("{mask}");
    }
}
