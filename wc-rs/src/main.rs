use rayon::prelude::*;
use std::fs::File;
use std::io::BufRead;
use std::io::BufReader;
use std::io::Error;
use std::str::from_utf8;

fn main() {
    let (newlines, characters, words) = std::env::args()
        .skip(1)
        .collect::<Vec<String>>()
        .par_iter()
        .flat_map(|x| count(&x))
        .reduce(|| (0, 0, 0), |x, y| (x.0 + y.0, x.1 + y.1, x.2 + y.2));

    println!("{}\t{}\t{}\ttotal", newlines, words, characters);
}

fn is_word_separator(byte: u8) -> bool {
    byte == b' ' || byte == b'\t' || byte == b'\r' || byte == 0x16_u8 || byte == 0x0C_u8
}

fn count(path: &str) -> Result<(usize, usize, usize), Error> {
    let mut file = BufReader::new(File::open(&path)?);

    let mut raw_line = Vec::new();
    let mut words: usize = 0;
    let mut newlines: usize = 0;
    let mut characters: usize = 0;

    while match file.read_until(b'\n', &mut raw_line) {
        Ok(n) if n > 0 => true,
        _ => false,
    } {
        if *raw_line.last().unwrap() == b'\n' {
            newlines += 1;
        }

        match from_utf8(&raw_line[..]) {
            Ok(line) => {
                words += line.split_whitespace().count();
                characters += line.chars().count();
            }
            Err(..) => {
                words += raw_line.split(|&x| is_word_separator(x)).count();
                characters += raw_line.iter().filter(|c| c.is_ascii()).count();
            }
        }
        raw_line.truncate(0);
    }

    println!("{}\t{}\t{}\t{}", newlines, words, characters, path);

    Ok((newlines, characters, words))
}
