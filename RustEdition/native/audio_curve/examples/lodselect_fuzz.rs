// Fuzz oracle for sync_lod_select: configures per line, prints one result per line.
// Line format: min1,div1;min2,div2|cullmin,culldiv,noorigindiv,hyst,high|ox,oy,oz,vdx,vdy,vdz|px,py,pz,prevband,prevculled,useslod,usesculling,hasorigin
// Output: band,source,div,high
// Not shipped in the cdylib; Temp-harness use only.
use audio_curve::{LodBandFlat, sync_lod_configure, sync_lod_select};
use std::io::{BufRead, BufReader};

fn main() {
    let path = std::env::args().nth(1).expect("usage: lodselect_fuzz <cases.txt>");
    let f = BufReader::new(std::fs::File::open(path).unwrap());
    for line in f.lines() {
        let line = line.unwrap();
        if line.trim().is_empty() {
            continue;
        }
        let p: Vec<&str> = line.split('|').collect();
        let bands: Vec<LodBandFlat> = if p[0].trim().is_empty() {
            Vec::new()
        } else {
            p[0].split(';')
                .map(|b| {
                    let q: Vec<&str> = b.split(',').collect();
                    LodBandFlat {
                        min_distance: q[0].parse().unwrap(),
                        tick_rate_divisor: q[1].parse().unwrap(),
                    }
                })
                .collect()
        };
        let c: Vec<&str> = p[1].split(',').collect();
        sync_lod_configure(
            bands.as_ptr(),
            bands.len() as i32,
            c[0].parse().unwrap(),
            c[1].parse().unwrap(),
            c[2].parse().unwrap(),
            c[3].parse().unwrap(),
            c[4].parse().unwrap(),
        );
        let f6 = |s: &str| {
            s.split(',')
                .map(|v| v.parse::<f32>().unwrap())
                .collect::<Vec<f32>>()
        };
        // Part 2 packs origin xyz + view xyz (6 floats); part 3 is params.
        let ov = f6(p[2]);
        let r: Vec<&str> = p[3].split(',').collect();
        let out = sync_lod_select(
            ov[0], ov[1], ov[2], ov[3], ov[4], ov[5], r[0].parse().unwrap(),
            r[1].parse().unwrap(), r[2].parse().unwrap(), r[3].parse().unwrap(),
            r[4].parse().unwrap(), r[5].parse().unwrap(), r[6].parse().unwrap(),
            r[7].parse().unwrap(),
        );
        println!("{},{},{},{}", out.band_index, out.source, out.tick_rate_divisor, out.use_high_precision);
    }
}
