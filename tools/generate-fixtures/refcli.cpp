#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <vector>
#include "api.h"

// usage: refcli c|d <level> <in> <out>
int main(int argc, char** argv) {
  if (argc != 5) { fprintf(stderr, "usage: refcli c|d level in out\n"); return 2; }
  FILE* f = fopen(argv[3], "rb");
  if (!f) { perror("in"); return 2; }
  std::vector<uint8_t> buf;
  uint8_t tmp[65536]; size_t n;
  while ((n = fread(tmp, 1, sizeof tmp, f)) > 0) buf.insert(buf.end(), tmp, tmp+n);
  fclose(f);
  AllocatedMemory r;
  if (argv[1][0]=='c') r = compress(buf.data(), buf.size(), (uint8_t)atoi(argv[2]));
  else r = decompress(buf.data(), buf.size(), (uint8_t)atoi(argv[2]));
  if (r.status != 0) { fprintf(stderr, "error status=%d msg=%s\n", r.status, r.data ? (char*)r.data : ""); return 1; }
  FILE* o = fopen(argv[4], "wb");
  fwrite(r.data, 1, r.length, o);
  fclose(o);
  return 0;
}
