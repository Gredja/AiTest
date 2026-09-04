import { readFileSync } from "fs"
import { join, basename } from "path"

export default {
  "experimental.chat.system.transform": async (input, output) => {
    try {
      const projectName = basename(process.cwd())
      const mem = readFileSync(
        join(process.env.USERPROFILE!, `.local/share/mimocode/memory/projects/${projectName}/MEMORY.md`),
        "utf-8"
      )
      output.system.push(mem)
    } catch {}
  },
}
