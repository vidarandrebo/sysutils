package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"sync"
)

type stats struct {
	words      int
	lines      int
	characters int
	gmu        sync.Mutex
}

func (s *stats) String() string {
	return fmt.Sprintf("%d\t%d\t%d", s.lines, s.words, s.characters)
}

func combine(oldStat, newStat *stats) {
	oldStat.words += newStat.words
	oldStat.lines += newStat.lines
	oldStat.characters += newStat.characters
}

func newStats() *stats {
	return &stats{}
}

func loadFile(fileName string) []byte {
	file, err := ioutil.ReadFile(fileName)
	if err != nil {
		log.Fatal(err)
	}
	return file
}

func wordCount(b []byte) *stats {
	count := newStats()
	var last bool
	last = false
	for _, c := range b {
		switch c {
		case '\n':
			count.lines++
			count.characters++
			if last {
				count.words++
				last = false
			}
		case ' ':
			count.characters++
			if last {
				count.words++
				last = false
			}
		case '\t':
			count.characters++
			if last {
				count.words++
				last = false
			}
		default:
			count.characters++
			last = true
		}
	}
	return count
}

func main() {
	total := newStats()
	var wg sync.WaitGroup
	wg.Add(len(os.Args) - 1)
	for i := 1; i < len(os.Args); i++ {
		go func(i int) {
			defer wg.Done()
			file := loadFile(os.Args[i])
			count := wordCount(file)
			total.gmu.Lock()
			combine(total, count)
			total.gmu.Unlock()
			fmt.Printf("%s\t%s\n", count.String(), os.Args[i])
		}(i)

	}
	wg.Wait()
	if len(os.Args) > 2 {
		fmt.Printf("%s\ttotal\n", total.String())
	} else if len(os.Args) == 1 {
		data, _ := ioutil.ReadFile("/dev/stdin")
		counter := wordCount(data)
		fmt.Printf("%s\n", counter.String())
	}
}
