import { readFileSync } from "fs"
import { join } from "path"

export default {
  "experimental.chat.system.transform": async (input, output) => {
    try {
      const mem = readFileSync(
        join(process.env.USERPROFILE!, ".local/share/mimocode/memory/projects/gredja/MEMORY.md"),
        "utf-8"
      )
      output.system.push(mem)
    } catch {}
  },
}
